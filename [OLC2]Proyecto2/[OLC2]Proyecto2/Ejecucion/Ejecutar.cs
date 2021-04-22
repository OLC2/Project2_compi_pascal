﻿using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace _OLC2_Proyecto2.Ejecucion
{
    class Heap
    {
        public String Alfanum;
        public int Ascii;

        public Heap(string alfanum, int ascii)
        {
            this.Alfanum = alfanum;
            this.Ascii = ascii;
        }
    }

    class Ejecutar
    {
        public TablaFunciones tablafunciones = new TablaFunciones();

        public Stack<TablaSimbolos> pilaSimbolos;
        private TablaSimbolos cima;                     //Esto tiene el ambito actual
        private TablaSimbolos cimaG;                    //Esto tiene el ambito de las variables globales

        private List<Parametro> lstParametros;          //Para los parametros de las funciones
        private List<Celda> arreglo;                    //Para los arreglos
        public List<String> lstPrint = new List<String>();
        public List<Error> lstError = new List<Error>();

        public List<Heap> HP = new List<Heap>();

        private Boolean BanderaCaso = false; //Controla que los casos no repitan sus condiciones

        private int Temporal = -1;
        private int Etiqueta = -1;
        private int Apuntador = -1;
        private int ApuntadorHP = -1;

        private string cadC3D = "";

        //private int ContadorParams = 0;
        private int nivelActual = 1; //Este controla el nivel que se estara consultando para crear, buscar y modificar las variables locales dentro de metodos, condiciones, ciclos, etc...

        public void IniciarPrimeraPasada(ParseTreeNode Nodo)
        {

            Debug.WriteLine("Ejecutando... Inserto TS - VariablesGlobales");

            IniciarEjecucion(Nodo);
        }

        public void IniciarEjecucion(ParseTreeNode Nodo)
        {
            pilaSimbolos = new Stack<TablaSimbolos>();

            //isRetornoG = false;

            TablaSimbolos varg = new TablaSimbolos(0, Reservada.variable, false, false);
            pilaSimbolos.Push(varg);
            cimaG = varg;

            cadC3D += ("void main() { //Initial Program\n");
            cadC3D += ("//*** Inicia Declaracion de Variables Globales *** \n");
            InitialProgram(Nodo);
            cadC3D += ("//*** Finaliza Declaracion de Variables Globales *** \n");
            EjecutarX();
            cadC3D += ("}\n\n");

            string rs = agregarEncabezado();
            Form1.Salida.AppendText(rs + "\n\n" + cadC3D);
        }

        private void InitialProgram(ParseTreeNode Nodo)
        {
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "S":
                        //S.Rule = ToTerm("program") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + punto
                        if (Nodo.ChildNodes.Count == 8)
                        {
                            tablafunciones.addFuncion(Reservada.Program, Reservada.Program, Reservada.Program, "nulo", "nulo", null, Nodo.ChildNodes[5], "0", "0");
                            Estructura(Nodo);
                        }
                        break;
                    default:
                        Debug.WriteLine("Error AST-->Nodo " + Nodo.Term.Name + " no existente/detectado");
                        break;
                }
            }
        }

        private void Estructura(ParseTreeNode Nodo)
        {
            /*
            S.Rule = ToTerm("program") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + punto

            ESTRUCTURA.Rule = ESTRUCTURA + BLOQUE | BLOQUE

            BLOQUE.Rule = VARYTYPE
                        | FUNCIONES
                        | PROCEDIMIENTO
                        | Empty //Vacio
            */
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "S":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            Estructura(hijo);
                        }
                        break;
                    case "ESTRUCTURA":
                        #region
                        if (Nodo.ChildNodes.Count == 2)
                        {
                            Estructura(Nodo.ChildNodes[0]); // ChildNodes[0] --> ESTRUCTURA
                            Estructura(Nodo.ChildNodes[1]); // ChildNodes[1] --> BLOQUE
                        }
                        else
                        {
                            Estructura(Nodo.ChildNodes[0]); // ChildNodes[0] --> BLOQUE
                        }
                        #endregion
                        break;
                    case "BLOQUE":
                        #region                        
                        switch (Nodo.ChildNodes[0].Term.Name)
                        {
                            case "VARYTYPE":
                                VariablesGlobales(Nodo.ChildNodes[0]); // ChildNodes[0] --> VARYTYPE
                                break;
                            case "FUNCIONES":
                                AlmacenarFuncion(Nodo.ChildNodes[0]);
                                break;
                            case "PROCEDIMIENTO":
                                AlmacenarProcedimiento(Nodo.ChildNodes[0]);
                                break;
                            default:
                                Debug.WriteLine("Error AST-->Nodo " + Nodo.Term.Name + " es empty/null");
                                break;
                        }
                        #endregion
                        break;
                    case "SENTENCIAS":
                        Debug.WriteLine("*** Iniciando Ejecucion de Sentencias ***");
                        break;
                }
            }
            else
            {
                Debug.WriteLine("Error AST-->Nodo en funcion Estructura no existente/detectado/null");
            }
        }

        private void AlmacenarFuncion(ParseTreeNode Nodo)
        {
            /*
             FUNCIONES.Rule = ToTerm("function") + id + parentA + PARAMETROS + parentC + dospuntos + TIPODATO + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            | ToTerm("function") + id + dospuntos + TIPODATO + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
             */
            Debug.WriteLine("ALMACENANDOFUNCION");


            String id = Nodo.ChildNodes[1].Token.Value.ToString();
            String tipodato = "";

            switch (Nodo.ChildNodes.Count)
            {
                case 13:
                    tipodato = getTipoDatoFunction(Nodo.ChildNodes[6]);

                    Funciones funct1 = tablafunciones.RetornarFuncion(id);

                    if (!tablafunciones.existeFuncionByKey(id))
                    {

                    }
                    else
                    {

                    }
                    /*
                    String claveMetodo = GetKey(id.Valor, Nodo);

                    if (!tipodato.Equals(Reservada.Void))
                    {
                        lstParametros = new List<Parametro>(); //Creando lista para parametros
                        AgregarParametros(Nodo.ChildNodes[4]); //Llenando lista de parametros
                        tablafunciones.addFuncion(claveMetodo, Reservada.Funcion, id.Valor, getInicialDato(tipodato), tipodato, lstParametros, Nodo.ChildNodes[7], getLinea(Nodo.ChildNodes[1]), "0");
                    }
                    else
                    {
                        lstParametros = new List<Parametro>(); //Creando lista para parametros
                        AgregarParametros(Nodo.ChildNodes[4]); //Llenando lista de parametros
                        tablafunciones.addFuncion(claveMetodo, Reservada.Funcion, id.Valor, "nulo", tipodato, lstParametros, Nodo.ChildNodes[7], getLinea(Nodo.ChildNodes[1]), "0");
                    }
                    */
                    break;
                case 10:
                    //ToTerm("function") + id + dospuntos + TIPODATO + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                    if(!tablafunciones.existeFuncionByKey(id))
                    {
                        tipodato = getTipoDatoFunction(Nodo.ChildNodes[3]);
                        tablafunciones.addFuncion(id, Reservada.Funcion, id, getInicialDato(tipodato), tipodato, null, Nodo.ChildNodes[7], getLinea(Nodo.ChildNodes[0]), "0");

                        Funciones funct2 = tablafunciones.RetornarFuncion(id);

                        if (funct2 != null)
                        {
                            TablaSimbolos fun = new TablaSimbolos(1, Reservada.Funcion, false, false); //Esto depende de si es VOID
                            pilaSimbolos.Push(fun);
                            cima = fun; //Estableciendo la tabla de simbolos cima
                            nivelActual++; //Estableciendo el nivel actual

                            string l_return = getEtiqueta();
                            cadC3D += ("void " + id + "() {\n");

                            functionC3D(Nodo.ChildNodes[5], null, Nodo.ChildNodes[7], l_return);

                            cadC3D += (l_return + ":\n");
                            cadC3D += ("return;\n");
                            cadC3D += ("}\n");

                            nivelActual--; //Disminuimos el nivel actual ya que salimos del metodo invocado
                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                            cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico-->Funcion no existente linea:" + funct2.getLinea() + " columna:" + funct2.getColumna());
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", funct2.getLinea(), funct2.getColumna()));
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Error Semantico-->Funcion ya existente linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion ya existente", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                    }
                    
                    break;
            }
        }

        private void functionC3D(ParseTreeNode varLocal, ParseTreeNode parametros, ParseTreeNode sentencias, string l_return)
        {
            cadC3D += ("ACA IRIA MI CODIGO INTERNO MAMADISIMO;\n");
        }

        private void AlmacenarProcedimiento(ParseTreeNode Nodo)
        {
            Debug.WriteLine("** Accion PROCEDIMIENTO no funcional");
        }

        private void VariablesGlobales(ParseTreeNode Nodo)
        {
            switch (Nodo.ChildNodes[0].Term.Name)
            {
                case "type":
                    Debug.WriteLine("** Accion type no funcional");
                    break;
                case "var": //ToTerm("var") + LSTVARS
                    LstVars(Reservada.variable, Nodo.ChildNodes[1]);
                    break;
                case "const":
                    Debug.WriteLine("** Accion const no funcional");
                    break;
                case "id":
                    Debug.WriteLine("** Accion id no funcional");
                    break;
                default:
                    Debug.WriteLine("Error AST-->Nodo en funcion Variable_y_type no existente/detectado");
                    break;
            }
        }

        private void LstVars(string tipoObj, ParseTreeNode Nodo)
        {
            switch (Nodo.Term.Name)
            {
                case "LSTVARS":
                    foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                    {
                        LstVars(tipoObj, hijo);
                    }
                    break;
                case "VARS":
                    Vars(tipoObj, Nodo);
                    break;
                default:
                    Debug.WriteLine("Error AST-->Nodo en funcion LstVars no existente/detectado");
                    break;
            }
        }

        private void Vars(string tipoObj, ParseTreeNode Nodo)
        {
            switch (Nodo.Term.Name)
            {
                case "VARS":
                    //LSTID + dospuntos + TIPODATO + puntocoma
                    if (Nodo.ChildNodes.Count == 4)
                    {
                        String td = getTipoDato(Nodo.ChildNodes[2]);
                        Retorno asignar = new Retorno(Reservada.nulo, Reservada.nulo, td, getInicialDato(td), getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                        DeclaracionAsignacionData(tipoObj, td, asignar, Nodo.ChildNodes[0]);
                    }
                    //LSTID + dospuntos + TIPODATO + ToTerm("=") + CONDICION + puntocoma
                    else if (Nodo.ChildNodes.Count == 6)
                    {
                        String td = getTipoDato(Nodo.ChildNodes[2]);
                        Retorno asignar = Condicion(Nodo.ChildNodes[4]);
                        DeclaracionAsignacionData(tipoObj, td, asignar, Nodo.ChildNodes[0]);
                    }
                    break;
                default:
                    Debug.WriteLine("Error AST-->Nodo en funcion Vars no existente/detectado");
                    break;
            }
        }

        private void DeclaracionAsignacionData(string tipoObj, String tipodato, Retorno ret, ParseTreeNode Nodo)
        {
            //METODO PARA LA DECLARACION DE VARIABLES
            /*
             LSTID.Rule = LSTID + coma + id
                        | id
             */
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "LSTID":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            DeclaracionAsignacionData(tipoObj, tipodato, ret, hijo);
                        }
                        break;

                    case "id":
                        String id = Nodo.Token.Value.ToString();
                        /*
                        Debug.WriteLine("LLEGO A RECONOCER LAS VARIABLES A DECLARAR PAPU");
                        Debug.WriteLine("nombre variable: " + id);
                        Debug.WriteLine("tipo objeto: " + tipoObj);
                        Debug.WriteLine("tipo dato: " + tipodato);
                        Debug.WriteLine("Valor asignable: " + ret.Valor.ToString());
                        */
                        if (!cimaG.existeSimbolo(id))
                        {
                            if (ret != null)
                            {
                                if (ret.Tipo.Equals(tipodato)) //Si son del mismo tipo se pueden asignar (variable con variable)
                                {
                                    Debug.WriteLine("Se creo variable: " + id + " --> " + ret.Valor + " (" + ret.Tipo + ")");

                                    int apuntador = newApuntador();

                                    if (!tipodato.Equals(Reservada.Cadena))
                                    {
                                        setStack(apuntador + "", ret, id);
                                        cimaG.addSimbolo(apuntador, Reservada.variable, id, ret.Valor, tipodato, Reservada.variable, getLinea(Nodo), getColumna(Nodo), true, null);
                                    }
                                    else
                                    {
                                        int apHP = newApuntadorHP();
                                        string tmp = getTemp();


                                        stringToHeap(tmp, apHP, ret.Valor);
                                        setStack(apuntador + "", new Retorno(tmp, "", Reservada.Cadena, ret.Valor, getLinea(Nodo), getColumna(Nodo)), id);
                                        cimaG.addSimbolo(apuntador, Reservada.variable, id, ret.Valor, tipodato, Reservada.variable, getLinea(Nodo), getColumna(Nodo), true, null);
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine("Error Semantico-->Asignacion no valida, tipo de dato incorrecto linea:" + getLinea(Nodo) + " columna:" + getColumna(Nodo));
                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, tipo de dato incorrecto", getLinea(Nodo), getColumna(Nodo)));
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Error Semantico-->Asignacion no valida, expresion incorrecta linea:" + getLinea(Nodo) + " columna:" + getColumna(Nodo));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, expresion incorrecta", getLinea(Nodo), getColumna(Nodo)));
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico-->Variable ya existente linea:" + getLinea(Nodo) + " columna:" + getColumna(Nodo));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Variable ya existente", getLinea(Nodo), getColumna(Nodo)));
                        }

                        break;

                    case ",": //No hace nada
                        break;

                    default:
                        Debug.WriteLine("Error AST-->Nodo en funcion DeclaracionAsignacionData no existente/detectado");
                        break;
                }
            }
            else
                Debug.WriteLine("Error AST-->Nodo en funcion DeclaracionAsignacionData no existente/detectado/null");
        }

        //****************************************************** ENTORNOS INTERNOS ******************************************************

        private void EjecutarX()
        {
            Funciones funcion = tablafunciones.RetornarFuncion(Reservada.Program);

            if (funcion != null)
            {
                #region

                TablaSimbolos program = new TablaSimbolos(1, Reservada.Program, false, false); //Esto depende de si es VOID
                pilaSimbolos.Push(program);
                cima = program; //Estableciendo la tabla de simbolos cima
                nivelActual = 1; //Estableciendo el nivel actual

                RetornoAc retorno = Sentencias(funcion.getCuerpo());

                nivelActual--; //Disminuimos el nivel actual ya que salimos del metodo invocado
                pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                
                #endregion
            }
            else
            {
                Debug.WriteLine("Error Semantico-->Funcion no existente linea:" + funcion.getLinea() + " columna:" + funcion.getColumna());
                lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", funcion.getLinea(), funcion.getColumna()));
            }
        }

        private RetornoAc Sentencias(ParseTreeNode Nodo)
        {
            //if (!isRetornoG)
            //{
            switch (Nodo.Term.Name)
            {
                case "SENTENCIAS":
                    foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                    {
                        RetornoAc retorno = Sentencias(hijo); // SENTENCIA | SENTENCIAS

                        if (retorno.Retorna && cima.Retorna)
                        {
                            return retorno;
                        }
                        else if (retorno.Detener && cima.Detener)
                        {
                            return retorno;
                        }
                    }
                    break;
                case "SENTENCIA":
                    /*
                        SENTENCIA.Rule = ToTerm("write") + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                            | ToTerm("writeln") + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                            | ToTerm("while") + CONDICION + ToTerm("do") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            | ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            | ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + ToTerm("else") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            | ToTerm("for") + id + ToTerm(":=") + TERMINALES + ToTerm("to") + TERMINALES + ToTerm("do") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            | ToTerm("continue") + puntocoma
                            | ToTerm("break") + puntocoma
                    */
                    #region
                    switch (Nodo.ChildNodes.Count)
                    {
                        case 2:
                            //ToTerm("continue") + puntocoma
                            //ToTerm("break") + puntocoma
                            #region
                            switch (Nodo.ChildNodes[0].Term.Name)
                            {
                                case "continue":
                                    //RetornoAc retornoR = new RetornoAc("-", "-", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                    //retornoR.Retorna = true;
                                    //return retornoR;
                                    break;
                                case "break":
                                    RetornoAc retornoB = new RetornoAc("-", "-", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                    retornoB.Detener = true;
                                    return retornoB;
                            }
                            #endregion
                            break;
                        case 4:
                            /*
                             * id + parentA + parentC + puntocoma
                             * id + ToTerm(":=") + CONDICION + puntocoma
                             * ToTerm("exit") + parentA + parentC + puntocoma
                             */
                            #region
                            if (Nodo.ChildNodes[0].Term.Name.Equals("id") && Nodo.ChildNodes[1].Term.Name.Equals(":="))
                            {
                                //id + ToTerm(":=") + CONDICION + puntocoma
                                #region
                                string id4 = Nodo.ChildNodes[0].Token.Value.ToString();

                                Simbolo var = RetornarSimbolo(id4); //Busco en mi nivel actual

                                if (var == null) //Si no existe en mi nivel actual busco en las globales
                                {
                                    var = cimaG.RetornarSimbolo(id4);
                                    Debug.WriteLine(">>> Se busco en las globales <<<");
                                }

                                if (var != null) //Si la variable existe
                                {
                                    Retorno ret = Condicion(Nodo.ChildNodes[2]);

                                    if (ret != null)
                                    {
                                        if (ret.Tipo.Equals(var.Tipo)) //Si son del mismo tipo se pueden asignar (variable con variable)
                                        {
                                            var.Valor = ret.Valor; // Asignamos el nuevo valor al id
                                            setStack(var.Apuntador + "", ret, id4);
                                        }
                                        #region ASIGNACION DE ARREGLO A ARREGLO
                                        /*
                                        else if (ret.Tipo.Equals(Reservada.arreglo) && var.TipoObjeto.Equals(Reservada.arreglo))
                                        {
                                            Simbolo arregloAsignar = RetornarSimbolo(ret.Valor); // ret.Valor contiene el nombre del arreglo a asignar

                                            if (arregloAsignar == null) //Si no existe en mi nivel actual busco en las globales
                                            {
                                                arregloAsignar = cimaG.RetornarSimbolo(ret.Valor);
                                                Debug.WriteLine(">>> Se busco en las globalbes <<<");
                                            }
                                            Debug.WriteLine(">>> SE RECONOCIO ASIGNACION DE ARREGLOS PRRONES <<<");

                                            if (arregloAsignar != null)
                                            {
                                                Debug.WriteLine(">>> SE RECONOCIO ASIGNACION DE ARREGLOS PRRONES <<<");
                                                Debug.WriteLine("Se asigno ARREGLO: " + id + " --> " + ret.Valor + " (" + ret.Tipo + ")");
                                                if (arregloAsignar.Tipo.Equals(var.Tipo))
                                                {
                                                    if (var.Arreglo.Count >= arregloAsignar.Arreglo.Count)
                                                    {
                                                        int i = 0;
                                                        foreach (Celda cel in arregloAsignar.Arreglo)
                                                        {
                                                            var.Arreglo.ElementAt(i).valor = arregloAsignar.Arreglo.ElementAt(i).valor;
                                                            i++;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        int i = 0;
                                                        foreach (Celda cel in var.Arreglo)
                                                        {
                                                            var.Arreglo.ElementAt(i).valor = arregloAsignar.Arreglo.ElementAt(i).valor;
                                                            i++;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Debug.WriteLine("Error Semantico-->Asignacion no valida, tipo de dato incorrecto linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, tipo de dato incorrecto", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                                }
                                            }
                                            else
                                            {
                                                Debug.WriteLine("Error Semantico-->Asignacion no valida de arreglo linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                                lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida de arreglo", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                            }
                                        }
                                        */
                                        #endregion
                                        else
                                        {
                                            Debug.WriteLine("Error Semantico-->Asignacion no valida, tipo de dato incorrecto linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, tipo de dato incorrecto", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Debug.WriteLine("Error Semantico-->Asignacion no valida, expresion incorrecta linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, expresion incorrecta", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine("Error Semantico-->Variable " + id4 + " no existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Variable " + id4 + " no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                }
                                #endregion
                            }
                            #endregion
                            break;
                        case 5: /*
                                 * ToTerm("repeat") + SENTENCIAS + ToTerm("until") + CONDICION + puntocoma
                                 * ToTerm("exit") + parentA + CONDICION + parentC + puntocoma
                                 * ToTerm("write") + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                                 * ToTerm("writeln") + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                                 * id + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                                 */
                            #region
                            switch (Nodo.ChildNodes[0].Term.Name)
                            {
                                case "repeat":
                                    //ToTerm("repeat") + SENTENCIAS + ToTerm("until") + CONDICION + puntocoma
                                    #region
                                    Retorno condW = Condicion(Nodo.ChildNodes[3]);

                                    #endregion
                                    break;
                                case "exit":
                                    #region
                                    Retorno retu = Condicion(Nodo.ChildNodes[2]);

                                    #endregion
                                    break;
                                case "write":
                                    Retorno retWrite = getCadenaPrint(Nodo.ChildNodes[2]);

                                    Form1.Impresiones.AppendText(retWrite.Valor);
                                    return new RetornoAc("-", "-", "0", "0");

                                case "writeln":
                                    Retorno retWriteln = getCadenaPrint(Nodo.ChildNodes[2]);
                                    cadC3D += ("printf(\"%c\", 10); //Backspace\n");

                                    Form1.Impresiones.AppendText(retWriteln.Valor + "\n");
                                    return new RetornoAc("-", "-", "0", "0");
                                case "id":
                                    #region
                                    // id + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                                    //PRIMERO OBTENGO LA CANTIDAD DE VALORES EN MIS PARAMETROS ACEPTADOS POR EL ARREGLO

                                    #endregion
                                    break;
                            }
                            #endregion
                            break;
                        case 7:
                            //ToTerm("while") + CONDICION + ToTerm("do") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            //ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            #region
                            switch (Nodo.ChildNodes[0].Term.Name)
                            {
                                case "if":
                                    #region
                                    Retorno cond8 = Condicion(Nodo.ChildNodes[1]);

                                    #endregion
                                    break;
                                case "while":
                                    #region
                                    Retorno cond7 = Condicion(Nodo.ChildNodes[1]);

                                    #endregion
                                    break;
                            }
                            #endregion
                            break;
                        case 11:
                            //ToTerm("for") + id + ToTerm(":=") + TERMINALES + ToTerm("to") + TERMINALES + ToTerm("do") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            //ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + ToTerm("else") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            #region
                            switch (Nodo.ChildNodes[0].Term.Name)
                            {
                                case "for":
                                    //ToTerm("for") + id + ToTerm(":=") + TERMINALES + ToTerm("to") + TERMINALES + ToTerm("do") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                                    #region
                                    String id15 = Nodo.ChildNodes[1].Token.Value.ToString();
                                    Simbolo var15 = RetornarSimbolo(id15);

                                    #endregion
                                    break;
                                case "if":
                                    //ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + ToTerm("else") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                                    #region
                                    Retorno cond11 = Condicion(Nodo.ChildNodes[1]);

                                    #endregion
                                    break;
                            }

                            #endregion
                            break;
                    }
                    #endregion
                    break;
            }
            return new RetornoAc("-", "-", "0", "0");
        }

        private Retorno Condicion(ParseTreeNode Nodo)
        {
            /*
            CONDICION.Rule = CONDICION + ToTerm("and") + COND1
                            | COND1
            COND1.Rule = COND1 + ToTerm("or") + COND2
                        | COND2
            COND2.Rule = ToTerm("not") + COND3
                        | COND3
            COND3.Rule = COND3 + ToTerm("<=") + COND4
                        | COND4
            COND4.Rule = COND4 + ToTerm(">=") + COND5
                        | COND5
            COND5.Rule = COND5 + ToTerm("<") + COND6
                        | COND6
            COND6.Rule = COND6 + ToTerm(">") + COND7
                        | COND7
            COND7.Rule = COND7 + ToTerm("=") + COND8
                        | COND8
            COND8.Rule = COND8 + ToTerm("<>") + EXPRESION
                        | EXPRESION
            */

            if (Nodo.ChildNodes.Count == 3)
            {
                switch (Nodo.ChildNodes[0].Term.Name)
                {
                    case "CONDICION": // MakePlusRule(CONDICION, ToTerm("and"), COND1);
                        #region
                        Retorno condB1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condB2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condB1 != null) && (condB2 != null)) // Si ambos son distintos de null entra
                        {
                            if (condB1.Tipo.Equals(Reservada.Booleano) && condB2.Tipo.Equals(Reservada.Booleano)) // si ambos son booleanos
                            {
                                if (condB1.Valor.Equals("True") && condB2.Valor.Equals("True")) // si ambos son true 
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condB1, ">=", condB2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condB1, ">=", condB2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Imposible evaluar condicion AND con valores no booleanos");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion AND con valores no booleanos", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion AND");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion AND", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND1": // MakePlusRule(COND1, ToTerm("or"), COND2);
                        #region
                        Retorno condA1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condA2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condA1 != null) && (condA2 != null)) // Si ambos son distintos de null entra
                        {
                            if (condA1.Tipo.Equals(Reservada.Booleano) && condA2.Tipo.Equals(Reservada.Booleano)) // si ambos son booleanos
                            {
                                if (condA1.Valor.Equals("False") && condA2.Valor.Equals("False")) // si ambos son false 
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condA1, "or", condA2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condA1, ">=", condA2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Imposible evaluar condicion OR con valores no booleanos");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion OR con valores no booleanos", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion OR");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion OR", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND3": // MakePlusRule(COND3, ToTerm("<="), COND4);
                        #region
                        Retorno condC1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condC2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condC1 != null) && (condC2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condC1.Tipo.Equals(Reservada.Entero) && condC2.Tipo.Equals(Reservada.Real)) ||  // si uno es entero y otro real
                                     (condC1.Tipo.Equals(Reservada.Real) && condC2.Tipo.Equals(Reservada.Entero)) ||   // si uno es real y otro entero
                                         (condC1.Tipo.Equals(Reservada.Entero) && condC2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son enteros
                                             (condC1.Tipo.Equals(Reservada.Real) && condC2.Tipo.Equals(Reservada.Real)))   // si ambos son real
                            {
                                double val1 = double.Parse(condC1.Valor);
                                double val2 = double.Parse(condC2.Valor);

                                if (val1 <= val2)
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condC1, "<=", condC2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condC1, "<=", condC2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else if ((condC1.Tipo.Equals(Reservada.Cadena) && condC2.Tipo.Equals(Reservada.Cadena)))    //Si ambos son String
                            {
                                int v1 = getCantAscii(condC1.Valor);
                                int v2 = getCantAscii(condC2.Valor);

                                if (v1 <= v2)
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condC1, "<=", condC2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condC1, "<=", condC2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else // valores no numericos
                            {
                                Debug.WriteLine("Imposible evaluar condicion <= con valores diferentes");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <= con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion <=");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <=", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND4": // MakePlusRule(COND4, ToTerm(">="), COND5);
                        #region
                        Retorno condD1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condD2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condD1 != null) && (condD2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condD1.Tipo.Equals(Reservada.Entero) && condD2.Tipo.Equals(Reservada.Real)) ||  // si uno es entero y otro real
                                     (condD1.Tipo.Equals(Reservada.Real) && condD2.Tipo.Equals(Reservada.Entero)) ||   // si uno es real y otro entero
                                         (condD1.Tipo.Equals(Reservada.Entero) && condD2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son enteros
                                             (condD1.Tipo.Equals(Reservada.Real) && condD2.Tipo.Equals(Reservada.Real)))   // si ambos son real
                            {
                                double val1 = double.Parse(condD1.Valor);
                                double val2 = double.Parse(condD2.Valor);

                                if (val1 >= val2)
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condD1, ">=", condD2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condD1, ">=", condD2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else if ((condD1.Tipo.Equals(Reservada.Cadena) && condD2.Tipo.Equals(Reservada.Cadena)))     //Si ambos son String
                            {
                                int v1 = getCantAscii(condD1.Valor);
                                int v2 = getCantAscii(condD2.Valor);

                                if (v1 >= v2)
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condD1, ">=", condD2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condD1, ">=", condD2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else // valores no numericos
                            {
                                Debug.WriteLine("Imposible evaluar condicion >= con valores diferentes");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion >= con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion >=");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion >=", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND5": // MakePlusRule(COND5, ToTerm("<"), COND6);
                        #region
                        Retorno condE1 = Condicion(Nodo.ChildNodes[0]);//COND6
                        Retorno condE2 = Condicion(Nodo.ChildNodes[2]);//COND7

                        if ((condE1 != null) && (condE2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condE1.Tipo.Equals(Reservada.Entero) && condE2.Tipo.Equals(Reservada.Real)) ||  // si uno es Entero y otro Real
                                     (condE1.Tipo.Equals(Reservada.Real) && condE2.Tipo.Equals(Reservada.Entero)) ||   // si uno es Real y otro Entero
                                         (condE1.Tipo.Equals(Reservada.Entero) && condE2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son Enteros
                                             (condE1.Tipo.Equals(Reservada.Real) && condE2.Tipo.Equals(Reservada.Real)))   // si ambos son Real
                            {
                                double val1 = double.Parse(condE1.Valor);
                                double val2 = double.Parse(condE2.Valor);

                                if (val1 < val2)
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condE1, "<", condE2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condE1, "<", condE2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else if ((condE1.Tipo.Equals(Reservada.Cadena) && condE2.Tipo.Equals(Reservada.Cadena)))     //Si ambos son String
                            {
                                int v1 = getCantAscii(condE1.Valor);
                                int v2 = getCantAscii(condE2.Valor);

                                if (v1 < v2)
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condE1, "<", condE2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condE1, "<", condE2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else // valores no numericos
                            {
                                Debug.WriteLine("Imposible evaluar condicion < con valores diferentes");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion < con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion <");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND6": // MakePlusRule(COND6, ToTerm(">"), COND7);
                        #region
                        Retorno condF1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condF2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condF1 != null) && (condF2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condF1.Tipo.Equals(Reservada.Entero) && condF2.Tipo.Equals(Reservada.Real)) ||  // si uno es Entero y otro Real
                                     (condF1.Tipo.Equals(Reservada.Real) && condF2.Tipo.Equals(Reservada.Entero)) ||   // si uno es Real y otro Entero
                                         (condF1.Tipo.Equals(Reservada.Entero) && condF2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son Enteros
                                             (condF1.Tipo.Equals(Reservada.Real) && condF2.Tipo.Equals(Reservada.Real)))   // si ambos son Real
                            {
                                double val1 = double.Parse(condF1.Valor);
                                double val2 = double.Parse(condF2.Valor);

                                if (val1 > val2)
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condF1, ">", condF2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno True
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condF1, ">", condF2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno False
                                }
                            }
                            else if ((condF1.Tipo.Equals(Reservada.Cadena) && condF2.Tipo.Equals(Reservada.Cadena)))     //Si ambos son String
                            {
                                int v1 = getCantAscii(condF1.Valor);
                                int v2 = getCantAscii(condF2.Valor);

                                if (v1 > v2)
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condF1, ">", condF2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condF1, ">", condF2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else // valores no numericos
                            {
                                Debug.WriteLine("Imposible evaluar condicion > con valores diferentes");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion > con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion >");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion >", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND7": // MakePlusRule(COND7, ToTerm("="), COND8);
                        #region
                        Retorno condG1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condG2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condG1 != null) && (condG2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condG1.Tipo.Equals(Reservada.Entero) && condG2.Tipo.Equals(Reservada.Real)) ||  // si uno es Entero y otro Real
                                     (condG1.Tipo.Equals(Reservada.Real) && condG2.Tipo.Equals(Reservada.Entero)) ||   // si uno es Real y otro Entero
                                         (condG1.Tipo.Equals(Reservada.Entero) && condG2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son Enteros
                                             (condG1.Tipo.Equals(Reservada.Real) && condG2.Tipo.Equals(Reservada.Real)))   // si ambos son Real
                            {
                                double val1 = double.Parse(condG1.Valor);
                                double val2 = double.Parse(condG2.Valor);

                                if (val1 == val2)
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condG1, "==", condG2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condG1, "==", condG2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else if ((condG1.Tipo.Equals(Reservada.Cadena) && condG2.Tipo.Equals(Reservada.Cadena)) ||      //Si ambos son String
                                    (condG1.Tipo.Equals(Reservada.Booleano) && condG2.Tipo.Equals(Reservada.Booleano)))     //Si ambos son Boolean
                            {
                                int v1 = getCantAscii(condG1.Valor);
                                int v2 = getCantAscii(condG2.Valor);

                                if (v1 == v2)
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condG1, "==", condG2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condG1, "==", condG2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else // valores no numericos
                            {
                                Debug.WriteLine("Imposible evaluar condicion = con valores diferentes");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion = con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion =");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion =", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND8": // MakePlusRule(COND8, ToTerm("<>"), EXPRESION);
                        #region
                        Retorno condH1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condH2 = Expresion(Nodo.ChildNodes[2]);

                        if ((condH1 != null) && (condH2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condH1.Tipo.Equals(Reservada.Entero) && condH2.Tipo.Equals(Reservada.Real)) ||  // si uno es Entero y otro Real
                                     (condH1.Tipo.Equals(Reservada.Real) && condH2.Tipo.Equals(Reservada.Entero)) ||   // si uno es Real y otro Entero
                                         (condH1.Tipo.Equals(Reservada.Entero) && condH2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son Enteros
                                             (condH1.Tipo.Equals(Reservada.Real) && condH2.Tipo.Equals(Reservada.Real)))   // si ambos son Real
                            {
                                double val1 = double.Parse(condH1.Valor);
                                double val2 = double.Parse(condH2.Valor);

                                if (val1 != val2)
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condH1, "<>", condH2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condH1, "<>", condH2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno True
                                }
                            }
                            else if ((condH1.Tipo.Equals(Reservada.Cadena) && condH2.Tipo.Equals(Reservada.Cadena)) ||      //Si ambos son String
                                    (condH1.Tipo.Equals(Reservada.Booleano) && condH2.Tipo.Equals(Reservada.Booleano)))     //Si ambos son Boolean
                            {
                                int v1 = getCantAscii(condH1.Valor);
                                int v2 = getCantAscii(condH2.Valor);

                                if (v1 != v2)
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condH1, "<>", condH2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string c3d = getC3D(tmp, condH1, "<>", condH2);
                                    return new Retorno(tmp, c3d, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else // valores no numericos
                            {
                                Debug.WriteLine("Imposible evaluar condicion <> con valores diferentes");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <> con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion <>");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <>", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                        #endregion

                }
            }
            // ToTerm("not") + COND3
            else if (Nodo.ChildNodes.Count == 2)
            {
                #region
                Retorno condB1 = Condicion(Nodo.ChildNodes[1]);

                if (condB1 != null)
                {
                    if (condB1.Tipo.Equals(Reservada.Booleano)) // si es booleano
                    {
                        if (condB1.Tipo.Equals("True")) // si es true 
                        {
                            return new Retorno(condB1.Temporal, condB1.C3D, Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])); //retorno False
                        }
                        else
                        {
                            return new Retorno(condB1.Temporal, condB1.C3D, Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])); //retorno True
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Imposible evaluar condicion NOT con valores no booleanos");
                        Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion NOT con valores no booleanos", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Imposible evaluar condicion NOT");
                    Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                    lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion NOT", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                    return null;
                }
                #endregion
            }
            else if (Nodo.ChildNodes.Count == 1)
            {
                if (Nodo.ChildNodes[0].Term.Name.Equals("EXPRESION")) //EXPRESION
                {
                    return Expresion(Nodo.ChildNodes[0]);
                }
                else // COND1, COND2, COND3, COND4.... CONDX
                {
                    return Condicion(Nodo.ChildNodes[0]);
                }
            }
            return null;
        }

        private Retorno Expresion(ParseTreeNode Nodo)
        {
            /*
            EXPRESION.Rule = EXPRESION + mas + EXP1
                            | EXP1
            EXP1.Rule = EXP1 + menos + EXP2 
                        | EXP2
            EXP2.Rule = EXP2 + por + EXP3
                        | EXP3
            EXP3.Rule = EXP3 + division + EXP4
                        | EXP4
            EXP4.Rule = EXP4 + modulo + TERMINALES
                        | TERMINALES
            */
            if (Nodo.ChildNodes.Count == 3)
            {
                Retorno ra1 = Expresion(Nodo.ChildNodes[0]);
                Retorno ra2 = Expresion(Nodo.ChildNodes[2]);
                String linea1 = getLinea(Nodo.ChildNodes[1]);
                String colum1 = getColumna(Nodo.ChildNodes[1]);

                switch (Nodo.ChildNodes[0].Term.Name)
                {
                    case "EXPRESION": // EXPRESION + mas + EXP1
                        #region
                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if (ra1.Tipo.Equals(Reservada.Cadena) || ra2.Tipo.Equals(Reservada.Cadena)) // Si alguno es String concateno
                            {
                                String concat = "";
                                //if (ra1.Tipo.Equals(Reservada.Cadena))
                                //{
                                //    concat = ra1.Valor + GetOperable(ra2);
                                //}
                                //else
                                //{
                                //    concat = GetOperable(ra1) + ra2.Valor;
                                //}
                                concat = GetOperable(ra1).Valor + GetOperable(ra2).Valor;
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "+", ra2);
                                return new Retorno(tmp, c3d, Reservada.Cadena, concat, linea1, colum1);
                            }
                            else if (ra1.Tipo.Equals(ra2.Tipo) && !ra1.Tipo.Equals(Reservada.Cadena)) // Si ambos son del mismo tipo y distinto de Cadena
                            {
                                double suma = double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);

                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "+", ra2);

                                if (ra1.Tipo.Equals(Reservada.Booleano))
                                {
                                    return new Retorno(tmp, c3d, Reservada.Booleano, suma + "", linea1, colum1);
                                }
                                else
                                {
                                    return new Retorno(tmp, c3d, ra1.Tipo, suma + "", linea1, colum1);
                                }
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real)) || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double suma = double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "+", ra2);
                                return new Retorno(tmp, c3d, Reservada.Real, suma + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero)) || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double suma = double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "+", ra2);
                                return new Retorno(tmp, c3d, Reservada.Entero, suma + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero)) || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real)))
                            {
                                double suma = double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "+", ra2);
                                return new Retorno(tmp, c3d, Reservada.Real, suma + "", linea1, colum1);
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para suma linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para suma", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            if (ra2 == null)
                            {

                            }
                            Debug.WriteLine("Error Semantico-->Expresion no operable(null) linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable(null)", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "EXP1": // EXP1 + menos + EXP2
                        #region
                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Real)) //Cualquier combinacion de estos valores da Real
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real)))
                            {
                                double resta = double.Parse(GetOperable(ra1).Valor) - double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "-", ra2);
                                return new Retorno(tmp, c3d, Reservada.Real, resta + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Entero)) //Cualquier combinacion de estos valores da Entero
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double resta = double.Parse(GetOperable(ra1).Valor) - double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "-", ra2);
                                return new Retorno(tmp, c3d, Reservada.Entero, resta + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Cadena)) //Cualquier combinacion de estos valores da Error
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Cadena)))
                            {
                                Debug.WriteLine("Error Semantico--> Error al restar linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para restar", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para restar linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para restar", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "EXP2": // EXP2 + por + EXP3
                        #region
                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Real)) //Cualquier combinacion de estos valores da Real
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real)))
                            {
                                double mul = double.Parse(GetOperable(ra1).Valor) * double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "*", ra2);
                                return new Retorno(tmp, c3d, Reservada.Real, mul + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Entero))     //Cualquier combinacion de estos valores da Entero
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double mul = double.Parse(GetOperable(ra1).Valor) * double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "*", ra2);
                                return new Retorno(tmp, c3d, Reservada.Entero, mul + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Cadena)) //Cualquier combinacion de estos valores da Error
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Cadena)))
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para multiplicar linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para multiplicar", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para multiplicar linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para multiplicar", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "EXP3": // EXP3 + division + EXP4
                        #region
                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Real)) //Cualquier combinacion de estos valores da Real
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double div = double.Parse(GetOperable(ra1).Valor) / double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "/", ra2);
                                return new Retorno(tmp, c3d, Reservada.Real, div + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Cadena)) //Cualquier combinacion de estos valores da Error
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Cadena)))
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para dividir linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para dividir", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para dividir linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para dividir", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "EXP4": // EXP4 + modulo + TERMINALES 
                        #region
                        ra2 = Terminales(Nodo.ChildNodes[2]); //Aca vuelvo a asignar a ra2 el valor del TERMINAL

                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Real)) //Cualquier combinacion de estos valores da Real
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double mod = double.Parse(GetOperable(ra1).Valor) % double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "%", ra2);
                                return new Retorno(tmp, c3d, Reservada.Real, mod + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Cadena)) //Cualquier combinacion de estos valores da Error
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Cadena)))
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para modulo linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para modulo", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para modulo linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para modulo", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                        #endregion
                }
            }
            else if (Nodo.ChildNodes.Count == 1)
            {
                if (Nodo.ChildNodes[0].Term.Name.Equals("TERMINALES"))
                {
                    return Terminales(Nodo.ChildNodes[0]);
                }
                else
                {
                    return Expresion(Nodo.ChildNodes[0]);
                }
            }
            return null;
        }

        private Retorno Terminales(ParseTreeNode Nodo)
        {
            /*
             TERMINALES.Rule = numero
                            | real
                            | menos + numero // Especial coco a esto
                            | menos + real // Especial coco a esto
                            | cadena
                            | ToTerm("true")
                            | ToTerm("false")
                            | id // Esto puede ser un ARREGLO, OBJECT o ?
                            | id + parentA + parentC // Invocacion funcion sin parametros
                            | id + parentA + ASIGNAR_PARAMETRO + parentC // Invocacion funcion con parametros
                            | id + corchA + ASIGNAR_PARAMETRO + corchC // Obteniendo valor de un array, condicion debe ser entero para acceder a esa posicion del arreglo
                            | parentA + CONDICION + parentC
             */
            switch (Nodo.ChildNodes.Count)
            {
                case 1:
                    //| numero
                    //| real
                    //| cadena
                    //| ToTerm("true")
                    //| ToTerm("false")
                    //| id // Esto puede ser un ARREGLO, OBJECT o ?
                    #region
                    switch (Nodo.ChildNodes[0].Term.Name)
                    {
                        case "numero":
                            return new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Entero, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "real":
                            return new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Real, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "cadena":
                            return new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Cadena, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "true":
                            return new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "false":
                            return new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "id": //Esto puede ser una VARIABLE o un ARREGLO

                            #region
                            String id = Nodo.ChildNodes[0].Token.Value.ToString();
                            Simbolo sim = RetornarSimbolo(id);

                            if (sim == null) //Si no existe en mi nivel actual busco en las globales
                            {
                                sim = cimaG.RetornarSimbolo(id);
                                Debug.WriteLine(">>> Se busco en las globales <<<");
                            }

                            if (sim != null)
                            {
                                //return new Retorno("xxx", "xxx", Reservada.Real, "5.5", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                                if (sim.TipoObjeto.Equals(Reservada.array))
                                {
                                    Debug.WriteLine("Operacion no completada!");
                                    return new Retorno("xxx", "xxx", Reservada.array, id, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    string cd3d = getStack(tmp, sim.Apuntador, id);
                                    return new Retorno(tmp, cd3d, sim.Tipo, sim.Valor, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Error Semantico-->Variable " + id + " no Existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Variable " + id + " no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                return null;
                            }
                            #endregion

                    }
                    #endregion
                    break;
                case 2:
                    //| menos + numero // Especial coco a esto
                    //| menos + real // Especial coco a esto
                    #region
                    switch (Nodo.ChildNodes[1].Term.Name)
                    {
                        case "numero":
                            Debug.WriteLine(Nodo.ChildNodes[0].Term.Name + Nodo.ChildNodes[1].Token.Value.ToString() + " <<================= SE ESTA RETORNANDO UN NEGATIVO BIEN PRRON");
                            return new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Entero, Nodo.ChildNodes[0].Term.Name + Nodo.ChildNodes[1].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                        case "real":
                            Debug.WriteLine(Nodo.ChildNodes[0].Term.Name + Nodo.ChildNodes[1].Token.Value.ToString() + " <<================= SE ESTA RETORNANDO UN NEGATIVO BIEN PRRON");
                            return new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Real, Nodo.ChildNodes[0].Term.Name + Nodo.ChildNodes[1].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                    }
                    #endregion
                    break;
                case 3:
                    //| id + parentA + parentC // Invocacion funcion sin parametros
                    //| parentA + CONDICION + parentC
                    #region
                    switch (Nodo.ChildNodes[0].Term.Name)
                    {
                        case "id": //| id + parentA + parentC // Invocacion funcion si parametros
                            /*
                            String id3 = Nodo.ChildNodes[0].Token.Value.ToString();
                            //Funciones func3 = tablafunciones.RetornarFuncion(id3);
                            Funciones func3 = tablafunciones.RetornarFuncionEvaluandoSobrecargaVoid(id3);

                            if (func3 != null)
                            {
                                if (!func3.getTipo().Equals(Reservada.Void)) //Si el metodo es de tipo VOID no retorna nada, ERROR
                                {
                                    nivelActual++; //Aumentamos el nivel actual ya que accedemos a otro metodo
                                    TablaSimbolos metodo4 = new TablaSimbolos(nivelActual, Reservada.Metodo, true, false, true);
                                    pilaSimbolos.Push(metodo4);
                                    cima = metodo4; //Estableciendo la tabla de simbolos cima

                                    Retorno reto = EjecutarFuncion(func3);

                                    nivelActual--; //Disminuimos el nivel actual ya que salimos del metodo invocado
                                    pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                    cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima

                                    return reto;// ORIGINALMENTE DEVOLVER ESTO PERRO
                                    //return new Retorno(Reservada.Cadena, nameUsuario, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                }
                                else
                                {
                                    Debug.WriteLine("Error Semantico-->Funcion no retorna valor linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no retorna valor", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Error Semantico-->Funcion no existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                            }
                            break;*/
                            return null; //BORRAR ESTHO!!!

                        case "(": //| parentA + CONDICION + parentC

                            Retorno ret = Condicion(Nodo.ChildNodes[1]);

                            if (ret != null)
                            {
                                return new Retorno(Reservada.nulo, Reservada.nulo, ret.Tipo, ret.Valor, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                            }
                            else
                            {
                                Debug.WriteLine("Error Semantico-->Retornno de parentesis mala linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Retornno de parentesis mala", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                            }
                            break;

                    }
                    #endregion
                    break;
                case 4:
                    //| id + parentA + ASIGNAR_PARAMETRO + parentC // Invocacion funcion con parametros
                    //| id + corchA + CONDICION + corchC // Obteniendo valor de un arreglo, condicion debe ser entero para acceder a esa posicion del arreglo

                    break;

                default:
                    return null;

            }
            return null;
        }

        private Retorno getCadenaPrint(ParseTreeNode Nodo)
        {
            string tmp = "";
            switch (Nodo.Term.Name)
            {
                case "ASIGNAR_PARAMETRO":
                    switch (Nodo.ChildNodes.Count)
                    {
                        case 3:

                            Retorno ret1 = getCadenaPrint(Nodo.ChildNodes[0]);
                            
                            Retorno ret2 = getCadenaPrint(Nodo.ChildNodes[2]);
                            /*
                            cadC3D += ("SP = SP + " + (Apuntador + 1) + ";\n"); //Aumentando apuntador
                            //tmp = getTemp();
                            setStack("SP", ret2, "Heap Pointer");
                            cadC3D += ("print_function();\n");
                            cadC3D += ("SP = SP - " + (Apuntador + 1) + ";\n"); //Disminuyendo apuntador
                            cadC3D += ("printf(\"%c\", 10);\n");
                            */
                            ret1.Valor = ret1.Valor + ret2.Valor;
                            ret1.Tipo = Reservada.Cadena;
                            return ret1;

                        case 1:
                            
                            ret1 = getCadenaPrint(Nodo.ChildNodes[0]);
                            /*
                            cadC3D += ("SP = SP + " + (Apuntador + 1) + ";\n"); //Aumentando apuntador
                            tmp = getTemp();
                            setStack("SP", ret1, "Heap Pointer");
                            cadC3D += ("print_function();\n");
                            cadC3D += ("SP = SP - " + (Apuntador + 1) + ";\n"); //Disminuyendo apuntador
                            cadC3D += ("printf(\"%c\", 10);\n");
                            */
                            return ret1;

                    }
                    break;
                case "CONDICION":
                    Retorno ret = Condicion(Nodo);
                    if (ret != null)
                    {
                        if (!ret.Temporal.Equals(Reservada.nulo))
                        {
                            if (ret.Tipo.Equals(Reservada.Cadena))
                            {
                                //Entra en este caso cuando lo que se quiere imprimir es una variable string
                                cadC3D += ("SP = SP + " + (Apuntador + 1) + ";\n"); //Aumentando apuntador
                                                                                    //tmp = getTemp();
                                setStack("SP", ret, "Heap Pointer");
                                cadC3D += ("print_function();\n");
                                cadC3D += ("SP = SP - " + (Apuntador + 1) + ";\n"); //Disminuyendo apuntador
                                //cadC3D += ("printf(\"%c\", 32); //Space\n");
                            }
                            else
                            {
                                cadC3D += ("printf(\"%f\", (float)" + ret.Temporal + ");\n");
                            }
                        }
                        else
                        {
                            //Entra en este caso cuando es una cadena dentro de writeln()
                            //int apuntador = newApuntador();
                            int apHP = newApuntadorHP();
                            tmp = getTemp();
                            stringToHeap(tmp, apHP, ret.Valor);
                            cadC3D += ("SP = SP + " + (Apuntador + 1) + ";\n");
                            cadC3D += ("Stack[(int)SP] = " + tmp + "; \t\t//Save cadena\n");
                            cadC3D += ("print_function();\n");
                            cadC3D += ("SP = SP - " + (Apuntador + 1) + ";\n");
                        }

                        return ret;
                    }
                    return null;
                default:
                    return null;
            }
            return null;
        }

        private string getTipoDatoFunction(ParseTreeNode Nodo)
        {
            switch (Nodo.ChildNodes[0].Term.Name)
            {
                case "String":
                    return Reservada.Cadena;
                case "Integer":
                    return Reservada.Entero;
                case "Real":
                    return Reservada.Real;
                case "Boolean":
                    return Reservada.Booleano;
            }
            return "Unknow";
        }

        private Retorno GetOperable(Retorno retornable)
        {
            switch (retornable.Tipo)
            {
                case "Char": //Cambia a ascii
                    retornable.Valor = GetAscii(retornable.Valor) + "";
                    return retornable;

                case "Boolean": //Cambia a 0 o 1
                    if (retornable.Valor.Equals("True"))
                    {
                        retornable.Valor = "1";
                        return retornable;
                    }
                    else
                    {
                        retornable.Valor = "0";
                        return retornable;
                    }

                default:
                    return retornable;
            }
        }

        private string getInicialDato(string tipodato)
        {
            if (tipodato.Equals(Reservada.Entero))
            {
                return "0";
            }
            else if (tipodato.Equals(Reservada.Real))
            {
                return "0.0";
            }
            else if (tipodato.Equals(Reservada.Cadena))
            {
                return "\"\"";
            }
            else if (tipodato.Equals(Reservada.Booleano))
            {
                return "False";
            }
            return "";
        }

        private string getTipoDato(ParseTreeNode Nodo)
        {
            switch (Nodo.ChildNodes[0].Term.Name)
            {
                case "Integer":
                    return "Integer";

                case "Real":
                    return "Real";

                case "Boolean":
                    return "Boolean";

                case "String":
                    return "String";

                default:
                    return "null";

            }
        }

        private int getCantAscii(String cadena)
        {
            Char[] Caracter = cadena.ToCharArray();

            int SumaAscii = 0;

            for (int i = 0; i < Caracter.Length; i++)
            {
                SumaAscii += GetAscii(Caracter[i] + "");
            }
            return SumaAscii;
        }

        private int GetAscii(String caracter)
        {
            return Encoding.ASCII.GetBytes(caracter)[0];
        }

        private string getC3D(string temp, Retorno izq, string op, Retorno der)
        {
            string rs = "";
            rs += temp + " = ";
            if (!izq.Temporal.Equals(Reservada.nulo))
            {
                rs += izq.Temporal;
            }
            else
            {
                rs += izq.Valor;
            }
            rs += " " + op + " ";
            if (!der.Temporal.Equals(Reservada.nulo))
            {
                rs += der.Temporal;
            }
            else
            {
                rs += der.Valor;
            }
            rs += ";\n";

            cadC3D += (rs);
            return rs;
        }

        private string getLinea(ParseTreeNode Nodo)
        {
            return (Nodo.Token.Location.Line + 1) + "";
        }

        private string getColumna(ParseTreeNode Nodo)
        {
            return (Nodo.Token.Location.Column + 1) + "";
        }

        //====================================================================================================== BUSQUEDAS AVANZADAS ===================================================================================

        private Boolean ExisteSimbolo(string nombre)
        {
            if (!Vacio())
            {
                foreach (TablaSimbolos ts in pilaSimbolos)
                {
                    if (ts.Nivel == nivelActual) //Busca el simbolo en el nivel que se maneja actualmente
                    {
                        //foreach (Simbolo simbolo in ts.getTS())
                        foreach (Simbolo simbolo in ts.ts)
                        {
                            if (simbolo.Nombre.Equals(nombre))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private Simbolo RetornarSimbolo(String nombre)
        {
            if (!Vacio())
            {
                foreach (TablaSimbolos ts in pilaSimbolos)
                {
                    if (ts.Nivel == nivelActual) //Busca el simbolo en el nivel que se maneja actualmente
                    {
                        foreach (Simbolo simbolo in ts.ts)
                        {
                            if (simbolo.Nombre.Equals(nombre))
                            {
                                return simbolo;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private Boolean Vacio()
        {
            if (!pilaSimbolos.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string agregarEncabezado()
        {
            string rs = "";
            rs = "#include <stdio.h> \t\t//Importar para el uso de printf \n";
            rs += "float Heap[100000]; \t\t//Estructura heap \n";
            rs += "float Stack[100000]; \t//Estructura stack \n";
            rs += "float SP; \t\t\t//Puntero Stack Pointer \n";
            rs += "float HP; \t\t\t//Puntero Heap Pointer \n";
            rs += "float N0, N1, N2; \t\t//Reservadas Print\n";
            rs += "\n//Declaración de temporales \n";
            rs += "float ";

            int i = 0;

            while (i <= Temporal)
            {
                rs += "T" + i;
                if (i != Temporal)
                {
                    rs += ", ";
                }
                i++;
            }
            rs += ";";

            rs += "\n\nvoid print_function() {\n";
            rs += "N0 = SP + 0;\n";
            rs += "N1 = Stack[(int)N0];\n";
            rs += "N2 = Heap[(int)N1];\n";
            rs += "L0:\n";
            rs += "if (N2 != -1) goto L1;\n";
            rs += "goto L2;\n";
            rs += "L1:\n";
            rs += "printf(\"%c\", (int)N2);\n";
            rs += "N1 = N1 + 1;\n";
            rs += "N2 = Heap[(int)N1];\n";
            rs += "goto L0;\n";
            rs += "L2:\n";
            rs += "return;\n";
            rs += "}\n";

            return rs;
        }

        private string getTemp()
        {
            //Crea y lleva el correlativo de los temporales creados
            Temporal++;
            return "T" + Temporal;
        }

        private string getEtiqueta()
        {
            //Crea y lleva el correlativo de los temporales creados
            Etiqueta++;
            return "L" + Etiqueta;
        }

        private int newApuntador()
        {
            //Crea y lleva el control del nuevo apuntador libre en el stack
            Apuntador++;
            Form1.Consola.AppendText("SP: " + Apuntador + "\n");
            return Apuntador;
        }

        private int newApuntadorHP()
        {
            //Crea y lleva el control del nuevo apuntador libre en el heap
            ApuntadorHP++;
            return ApuntadorHP;
        }

        private void setStack(String apuntad, Retorno res, string comment)
        {
            if (!res.Temporal.Equals(Reservada.nulo))
            {
                cadC3D += ("Stack[(int)" + apuntad + "] = " + res.Temporal + "; \t\t//Save " + comment + "\n");
            }
            else
            {
                cadC3D += ("Stack[(int)" + apuntad + "] = " + res.Valor + "; \t\t//Save " + comment + "\n");
            }
        }

        private string getStack(string varTemp, int apuntador, string comment)
        {
            string rs = varTemp + " = Stack[(int)" + apuntador + "]; \t\t//Get " + comment + "\n";
            cadC3D += (rs);
            return rs;
        }

        private void stringToHeap(string tmp, int apHP, string val)
        {
            char[] arr;
            arr = val.ToCharArray();

            //cadC3D += (tmp + " = HP_" + apHP + "; \t\t//Guardando cadena \n");
            cadC3D += (tmp + " = HP + 0; \t\t//Guardando cadena \n");

            foreach (char ch in arr)
            {
                //cadC3D += ("Heap[(int)HP_" + apHP + "] = " + GetAscii(ch + "") + "; \t\t//Char " + ch + "\n");
                cadC3D += ("Heap[(int)HP] = " + GetAscii(ch + "") + "; \t\t//Char " + ch + "\n");
                cadC3D += ("HP = HP + 1;\n");
                apHP = newApuntadorHP(); //Aumento el apuntador del Heap

                /*
                 * El siguiente codigo es solo de apoyo a mi, Eliminar si es necesario
                HP.Add(new Heap(ch + "", GetAscii(ch + "")));
                Form1.Consola.AppendText("HEAP[" + ch + "" + "," + GetAscii(ch + "") + "]\n");
                */
            }

            //cadC3D += ("Heap[(int)HP_" + apHP + "] = -1;\n");
            cadC3D += ("Heap[(int)HP] = -1;\n");
            cadC3D += ("HP = HP + 1;\n");
            newApuntadorHP(); //Aumento el apuntador del Heap
        }

    }
}
