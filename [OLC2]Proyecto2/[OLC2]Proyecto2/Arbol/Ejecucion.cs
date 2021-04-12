using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using _OLC2_Proyecto2.Ejecucion;
using Irony.Parsing;

namespace _OLC2_Proyecto2.Arbol
{
    class Ejecucion
    {
        private static List<Entorno> entorno;
        private Entorno cimaEnt;
        public Stack<Entorno> pilaEntornos;

        private List<Celda> arreglo;                    //Para los parametros
        public static List<Error> lstError = new List<Error>();
        
        public Stack<TablaSimbolos> pilaSimbolos;
        
        private TablaSimbolos cimaTS;
        private int nivelActual = 0; //Este controla el nivel que se estara consultando para crear, buscar y modificar las variables locales dentro de metodos, condiciones, ciclos, etc...


        public Ejecucion(List<Entorno> ent)
        {
            entorno = ent;
            pilaSimbolos = new Stack<TablaSimbolos>();
            pilaEntornos = new Stack<Entorno>();
        }

        public void Procedure()
        {
            cimaEnt = getEntorno(entorno[0].Ambito, entorno[0].AmbitoPadre, entorno);
            pilaEntornos.Push(cimaEnt);
            ejecutar(cimaEnt.nodoBegin);
        }

        private void ejecutar(ParseTreeNode Nodo)
        {
            TablaSimbolos proc = new TablaSimbolos(0, Reservada.Program, false, false);
            pilaSimbolos.Push(proc);
            cimaTS = proc;        //Estableciendo la tabla de simbolos cima
            nivelActual = 0;    //Estableciendo el nivel actual

            RetornoAc retorno = Sentencias(Nodo);
            
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
                        
                        if (retorno.Retorna && cimaEnt.IsRetorno)
                        {
                            return retorno;
                        }
                        else if (retorno.Detener && cimaTS.Detener)
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
                            if (Nodo.ChildNodes[0].Term.Name.Equals("id") && Nodo.ChildNodes[1].Term.Name.Equals("("))
                            {
                                //id + parentA + parentC + puntocoma
                                string id4 = Nodo.ChildNodes[0].Token.Value.ToString();
                                Entorno ent4 = getEntornoLocal(id4, cimaEnt.AmbitoPadre);
                                
                                if (ent4 != null)
                                {
                                    int cantParams = 0;
                                    foreach (Simbolo sim in ent4.variables)
                                    {
                                        if(sim.TipoObjeto.Equals(Reservada.parametro))
                                        {
                                            cantParams++;
                                        }
                                    }

                                    if(cantParams == 0)
                                    {
                                        nivelActual++;    //Estableciendo el nivel actual
                                        TablaSimbolos proc = new TablaSimbolos(0, Reservada.Procedure, cimaEnt.IsRetorno, false);
                                        pilaSimbolos.Push(proc);
                                        pilaEntornos.Push(ent4);
                                        cimaTS = proc;          //Estableciendo la tabla de simbolos cima
                                        cimaEnt = ent4;         //Estableciendo entorno actual
                                        cimaEnt.Activo = true;

                                        Sentencias(cimaEnt.nodoBegin);

                                        cimaEnt.Activo = false;
                                        nivelActual--; //Disminuimos el nivel actual ya que salimos del metodo invocado
                                        pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                        pilaEntornos.Pop();
                                        cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                        cimaEnt = pilaEntornos.Peek();

                                        return new RetornoAc("-", "-", "0", "0");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Parametros incorrectos linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Parametros incorrectos", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Error Semantico-->Function/Procedure no existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Function/Procedure no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                }
                            }
                            else if(Nodo.ChildNodes[0].Term.Name.Equals("exit") && Nodo.ChildNodes[1].Term.Name.Equals("(")) 
                            {
                                //ToTerm("exit") + parentA + parentC + puntocoma
                                RetornoAc retornoR = new RetornoAc(cimaEnt.TipoDatoRetorno, getInicialDato(cimaEnt.TipoDatoRetorno), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                retornoR.Retorna = true;
                                return retornoR;
                            }
                            else if(Nodo.ChildNodes[0].Term.Name.Equals("id") && Nodo.ChildNodes[1].Term.Name.Equals(":="))
                            {
                                //id + ToTerm(":=") + CONDICION + puntocoma
                                #region
                                string id4 = Nodo.ChildNodes[0].Token.Value.ToString();

                                if (id4.Equals(cimaEnt.Ambito))
                                {
                                    //Retorno de valor a traves del nombre de la function actual
                                    Retorno retu = Condicion(Nodo.ChildNodes[2]);

                                    if (retu != null)
                                    {
                                        if (retu.Tipo.Equals(cimaEnt.TipoDatoRetorno))
                                        {
                                            RetornoAc retornoV = new RetornoAc(retu.Tipo, retu.Valor, retu.Linea, retu.Columna);
                                            retornoV.Retorna = true;
                                            return retornoV;
                                        }
                                        else
                                        {
                                            //Console.WriteLine("Error Semantico-->Tipo de returno incorrecto linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Tipo de returno incorrecto", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Retono de expresion incorrecta linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Retono de expresion incorrecta", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                }
                                else
                                {
                                    //Asignacion de valor a una variable
                                    Simbolo var = RetornarSimbolo(id4); //Busco en mi nivel actual

                                    if (var != null) //Si la variable existe
                                    {
                                        Retorno ret = Condicion(Nodo.ChildNodes[2]);

                                        if (ret != null)
                                        {
                                            if (ret.Tipo.Equals(var.Tipo)) //Si son del mismo tipo se pueden asignar (variable con variable)
                                            {
                                                //Console.WriteLine("Se asigno variable: " + id4 + " --> " + ret.Valor + " (" + ret.Tipo + ")");
                                                var.Valor = ret.Valor; // Asignamos el nuevo valor a la variable
                                            }
                                            else
                                            {
                                                Console.WriteLine("Error Semantico-->Asignacion no valida, tipo de dato incorrecto linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                                lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, tipo de dato incorrecto", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Error Semantico-->Asignacion no valida, expresion incorrecta linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, expresion incorrecta", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Variable " + id4 + " no existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Variable " + id4 + " no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                }                                
                                #endregion
                            }
                            break;
                            #endregion
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

                                    if (condW != null)
                                    {
                                        if (condW.Tipo.Equals(Reservada.Booleano)) // Si la condicion es booleana
                                        {
                                            TablaSimbolos dowhilee = new TablaSimbolos(nivelActual, Reservada.Repeat, cimaEnt.IsRetorno, true);
                                            pilaSimbolos.Push(dowhilee);
                                            cimaTS = dowhilee; //Estableciendo la tabla de simbolos cima
                                                             //nivelActual = 1; //Estableciendo el nivel actual <<-- No en este caso de Do-While

                                            RetornoAc ret1 = Sentencias(Nodo.ChildNodes[1]); // Las sentencias se ejecutan al menos una vez en el Do-While

                                            if (ret1.Retorna)
                                            {
                                                pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                return ret1;
                                            }
                                            else if (ret1.Detener)
                                            {
                                                pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                break;
                                            }

                                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                            cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima

                                            if (condW.Valor.Equals("False"))
                                            {
                                                int contador = 1;

                                                while (true)
                                                {
                                                    TablaSimbolos dowhileee = new TablaSimbolos(nivelActual, Reservada.Repeat, cimaEnt.IsRetorno, true);
                                                    pilaSimbolos.Push(dowhileee);
                                                    cimaTS = dowhileee; //Estableciendo la tabla de simbolos cima
                                                                      //nivelActual = 1; //Estableciendo el nivel actual <<-- No en este caso de Do-While

                                                    ret1 = Sentencias(Nodo.ChildNodes[1]);

                                                    condW = Condicion(Nodo.ChildNodes[3]); // Esta condicion se vuelve a evaluar, como las variables se actualizan entonces el resultado de la condicion cambia

                                                    if (ret1.Retorna)
                                                    {
                                                        pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                        cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                        return ret1;
                                                    }
                                                    else if (ret1.Detener)
                                                    {
                                                        pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                        cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                        return ret1;
                                                    }

                                                    if ((condW.Valor.Equals("True")) || (contador == 50)) // Si la condicion es falsa // Maximo de iteraciones del do-while es 50
                                                    {
                                                        pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                        cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                        break;
                                                    }
                                                    contador++;

                                                    pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                    cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                }
                                            }
                                            return new RetornoAc("-", "-", "0", "0");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Valor de condicion invalida");
                                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[4]) + " columna:" + getColumna(Nodo.ChildNodes[4]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Valor de condicion invalida", getLinea(Nodo.ChildNodes[4]), getColumna(Nodo.ChildNodes[4])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Condicion invalida");
                                        Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[4]) + " columna:" + getColumna(Nodo.ChildNodes[4]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Condicion invalida", getLinea(Nodo.ChildNodes[4]), getColumna(Nodo.ChildNodes[4])));
                                    }
                                    #endregion
                                    break;
                                case "exit":
                                    #region
                                    Retorno retu = Condicion(Nodo.ChildNodes[2]);

                                    if (retu != null)
                                    {
                                        if(retu.Tipo.Equals(cimaEnt.TipoDatoRetorno))
                                        {
                                            RetornoAc retornoV = new RetornoAc(retu.Tipo, retu.Valor, retu.Linea, retu.Columna);
                                            retornoV.Retorna = true;
                                            return retornoV;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Error Semantico-->Tipo de returno incorrecto linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Tipo de returno incorrecto", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Retono de expresion incorrecta linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Retono de expresion incorrecta", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                    #endregion
                                    break;
                                case "write":
                                    string retWrite = getCadenaPrint(Nodo.ChildNodes[2]);

                                    Form1.Salida.AppendText(retWrite);
                                    Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++PRINT++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                                    return new RetornoAc("-", "-", "0", "0");
                                    
                                case "writeln":
                                    string retWriteln = getCadenaPrint(Nodo.ChildNodes[2]);

                                    Form1.Salida.AppendText(retWriteln+"\n");
                                    Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++PRINT++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                                    return new RetornoAc("-", "-", "0", "0");
                                case "id":
                                    #region
                                    // id + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                                    //PRIMERO OBTENGO LA CANTIDAD DE VALORES EN MIS PARAMETROS ACEPTADOS POR EL ARREGLO
                                    arreglo = new List<Celda>(); //Este arreglo es para almacenar los parametros del metodo que se invoco
                                    ValidarParametrosMetodo(Nodo.ChildNodes[2]); // Mandamos los parametros
                                    //AHORA BUSCO LA FUNCION EN BASE AL NOMBRE Y A MI ARREGLO DE PARAMETROS
                                    String id4 = Nodo.ChildNodes[0].Token.Value.ToString();
                                    
                                    Entorno ent4 = getEntornoLocal(id4, cimaEnt.AmbitoPadre);

                                    if (ent4 != null)
                                    {
                                        ent4 = EvaluarEnvioParametros(ent4, arreglo);

                                        if (ent4 != null) 
                                        {
                                            nivelActual++;          //Aumentamos el nivel actual ya que accedemos a otro metodo
                                            TablaSimbolos metodo4 = new TablaSimbolos(nivelActual, Reservada.Funcion, cimaEnt.IsRetorno, true);
                                            pilaSimbolos.Push(metodo4);
                                            pilaEntornos.Push(ent4);
                                            cimaTS = metodo4;       //Estableciendo la tabla de simbolos cima
                                            cimaEnt = ent4;         //Estableciendo entorno actual
                                            cimaEnt.Activo = true;

                                            Sentencias(cimaEnt.nodoBegin);

                                            ResetParametros(cimaEnt);

                                            cimaEnt.Activo = false;
                                            nivelActual--; //Disminuimos el nivel actual ya que salimos del metodo invocado
                                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                            pilaEntornos.Pop();
                                            cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                            cimaEnt = pilaEntornos.Peek();

                                            return new RetornoAc("-", "-", "0", "0");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Error Semantico-->Parametros de funcion no coinciden (2) linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Parametros de funcion no coinciden (2)", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Function/Procedure no existente (1) linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Function/Procedure no existente (1)", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
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

                                    if (cond8 != null)
                                    {
                                        if (cond8.Tipo.Equals(Reservada.Booleano)) // Si la condicion es booleana
                                        {
                                            if (cond8.Valor.Equals("True"))
                                            {
                                                TablaSimbolos iff = new TablaSimbolos(nivelActual, Reservada.Iff, cimaEnt.IsRetorno, cimaTS.Detener);
                                                pilaSimbolos.Push(iff);
                                                cimaTS = iff; //Estableciendo la tabla de simbolos cima
                                                            //nivelActual = 1; //Estableciendo el nivel actual <<-- No en este caso de If

                                                RetornoAc ret1 = Sentencias(Nodo.ChildNodes[4]);

                                                if (ret1.Retorna && cimaEnt.IsRetorno)
                                                {
                                                    pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                    cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                    return ret1;
                                                }
                                                else if (ret1.Detener && cimaTS.Detener)
                                                {
                                                    pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                    cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                    return ret1;
                                                }

                                                pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                return new RetornoAc("-", "-", "0", "0");
                                            }
                                            return new RetornoAc("-", "-", "0", "0");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Error Semantico-->Valor de condicion invalida linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Valor de condicion invalida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Condicion invalida linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Condicion invalida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                    #endregion
                                    break;
                                case "while":
                                    #region
                                    Retorno cond7 = Condicion(Nodo.ChildNodes[1]);

                                    if (cond7 != null)
                                    {
                                        if (cond7.Tipo.Equals(Reservada.Booleano)) // Si la condicion es booleana
                                        {
                                            if (cond7.Valor.Equals("True"))
                                            {
                                                int contador = 1;
                                                while (true)
                                                {
                                                    TablaSimbolos whilee = new TablaSimbolos(nivelActual, Reservada.Whilee, cimaEnt.IsRetorno, true);
                                                    pilaSimbolos.Push(whilee);
                                                    cimaTS = whilee; //Estableciendo la tabla de simbolos cima

                                                    RetornoAc ret1 = Sentencias(Nodo.ChildNodes[4]);
                                                    cond7 = Condicion(Nodo.ChildNodes[1]); // Esta condicion se vuelve a evaluar, como las variables se actualizan entonces el resultado de la condicion cambia

                                                    if (ret1.Retorna)
                                                    {
                                                        pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                        cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                        return ret1;
                                                    }
                                                    else if (ret1.Detener)
                                                    {
                                                        pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                        cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                        //return ret1;
                                                        break;
                                                    }

                                                    if ((cond7.Valor.Equals("False")) || (contador == 50)) // Si la condicion es falsa // Maximo de iteraciones del while es 50
                                                    {
                                                        pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                        cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                        break;
                                                    }

                                                    contador++;
                                                    pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                    cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                }
                                                return new RetornoAc("-", "-", "0", "0");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Valor de condicion invalida");
                                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Valor de condicion invalida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Condicion invalida");
                                        Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Condicion invalida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
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

                                    if (var15 != null) //Si mi asignacion de variable es distinta de null
                                    {
                                        Retorno ret15 = Terminales(Nodo.ChildNodes[3]);
                                        Retorno condicional = Terminales(Nodo.ChildNodes[5]);

                                        if (ret15.Tipo.Equals(var15.Tipo) && (ret15.Tipo.Equals(Reservada.Real) || ret15.Tipo.Equals(Reservada.Entero))) //Si son del mismo tipo se pueden asignar (variable con expresion)
                                        {
                                            var15.Valor = ret15.Valor;  // Asignamos el nuevo valor a la variable

                                            if (condicional.Tipo.Equals(Reservada.Entero) || condicional.Tipo.Equals(Reservada.Real))
                                            {
                                                if (!condicionFor(var15, condicional)) //Si la condicion es True se ejecutan las sentencias
                                                {
                                                    int contador = 0;

                                                    while (true)
                                                    {
                                                        TablaSimbolos forr = new TablaSimbolos(nivelActual, Reservada.Forr, cimaEnt.IsRetorno, true);
                                                        pilaSimbolos.Push(forr);
                                                        cimaTS = forr; //Estableciendo la tabla de simbolos cima

                                                        RetornoAc ret1 = Sentencias(Nodo.ChildNodes[8]); //Ejecuta sentencias
                                                        if (ret1.Retorna)
                                                        {
                                                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                            cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                            return ret1;
                                                        }
                                                        else if (ret1.Detener)
                                                        {
                                                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                            cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                                                          //return ret1;
                                                            break;
                                                        }

                                                        Simbolo inc = IncrementoFor(var15); //Ejecuta operacion incrementa/decremento

                                                        if (inc == null)
                                                        {
                                                            //Si entra aca significa que trono
                                                            return new RetornoAc("-", "-", "0", "0");
                                                        }
                                                        var15 = inc;

                                                        if (condicionFor(var15, condicional) || (contador == 50)) // Si la condicion es falsa // Maximo de iteraciones del For es 50
                                                        {
                                                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                            cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                            break;
                                                        }

                                                        contador++;

                                                        pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                        cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                    }
                                                    return new RetornoAc("-", "-", "0", "0");
                                                }
                                                return new RetornoAc("-", "-", "0", "0");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Error Semantico-->Tipo de condicional incorrecta linea:" + getLinea(Nodo.ChildNodes[4]) + " columna:" + getColumna(Nodo.ChildNodes[4]));
                                                lstError.Add(new Error(Reservada.ErrorSemantico, "Tipo de condicional incorrecta", getLinea(Nodo.ChildNodes[4]), getColumna(Nodo.ChildNodes[4])));
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Error Semantico-->Asignacion no valida, tipo de dato incorrecto linea:" + getLinea(Nodo.ChildNodes[2]) + " columna:" + getColumna(Nodo.ChildNodes[2]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, tipo de dato incorrecto", getLinea(Nodo.ChildNodes[2]), getColumna(Nodo.ChildNodes[2])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Variable no existente linea:" + getLinea(Nodo.ChildNodes[2]) + " columna:" + getColumna(Nodo.ChildNodes[2]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Variable no existente incorrecta", getLinea(Nodo.ChildNodes[2]), getColumna(Nodo.ChildNodes[2])));
                                    }
                                    #endregion
                                    break;
                                case "if":
                                    //ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + ToTerm("else") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                                    #region
                                    Retorno cond11 = Condicion(Nodo.ChildNodes[1]);

                                    if (cond11 != null)
                                    {
                                        if (cond11.Tipo.Equals(Reservada.Booleano)) // Si la condicion es booleana
                                        {
                                            TablaSimbolos iff = new TablaSimbolos(nivelActual, Reservada.Iff, cimaEnt.IsRetorno, cimaTS.Detener);
                                            pilaSimbolos.Push(iff);
                                            cimaTS = iff; //Estableciendo la tabla de simbolos cima
                                                        //nivelActual = 1; //Estableciendo el nivel actual <<-- No en este caso de If

                                            if (cond11.Valor.Equals("True"))
                                            {
                                                RetornoAc ret1 = Sentencias(Nodo.ChildNodes[4]); //Ejecutando sentencias del If

                                                if (ret1.Retorna && cimaEnt.IsRetorno)
                                                {
                                                    pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                    cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                    return ret1;
                                                }
                                                else if (ret1.Detener && cimaTS.Detener)
                                                {
                                                    pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                    cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                    return ret1;
                                                }

                                                pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima

                                                return new RetornoAc("-", "-", "0", "0");
                                            }
                                            else
                                            {
                                                RetornoAc ret1 = Sentencias(Nodo.ChildNodes[8]); //Ejecutando sentencias del Else

                                                if (ret1.Retorna && cimaEnt.IsRetorno)
                                                {
                                                    pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                    cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                    return ret1;
                                                }
                                                else if (ret1.Detener && cimaTS.Detener)
                                                {
                                                    pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                    cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                                    return ret1;
                                                }

                                                pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima

                                                return new RetornoAc("-", "-", "0", "0");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Valor de condicion invalida");
                                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Valor de condicion invalida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Condicion invalida");
                                        Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Condicion invalida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
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

        private Boolean condicionFor(Simbolo var, Retorno condicional)
        {
            if (var.Tipo.Equals(Reservada.Real) || var.Tipo.Equals(Reservada.Entero))
            {
                if (condicional.Tipo.Equals(Reservada.Real) || condicional.Tipo.Equals(Reservada.Entero))
                {
                    Double var1 = Double.Parse(var.Valor);
                    Double cond1 = Double.Parse(condicional.Valor);
                    if(var1 > cond1)
                    {
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        private Simbolo IncrementoFor(Simbolo var)
        {
            if(var.Tipo.Equals(Reservada.Entero))
            {
                int oper = Int32.Parse(var.Valor);
                oper = oper + 1;
                var.Valor = oper + "";
                return var;
            }
            if (var.Tipo.Equals(Reservada.Entero))
            {
                Double oper = Double.Parse(var.Valor);
                oper = oper + 1;
                var.Valor = oper + "";
                return var;
            }
            return null;
        }

        private Entorno EvaluarEnvioParametros(Entorno ent, List<Celda> arreglo)
        {
            if (ent.variables.Count > 0)
            {
                int cant = 0;
                foreach (Simbolo sim in ent.variables)
                {
                    if (sim.TipoObjeto.Equals(Reservada.parametro))
                    {
                        cant++;
                    }
                }
                if (cant == 0) //No tiene ningun parametro
                {
                    return null;
                }

                if (cant == arreglo.Count)
                {
                    int cont = 0;

                    foreach (Simbolo sim in ent.variables)
                    {
                        if (sim.TipoObjeto.Equals(Reservada.parametro))
                        {
                            if (sim.Tipo.Equals(arreglo[cont].tipo))
                            {
                                sim.Valor = arreglo[cont].valor;
                            }
                            cont++;
                        }                        
                    }

                    if (cant == cont)
                    {
                        return ent;
                    }
                    ResetParametros(ent);
                    return null;
                }
            }
            return null; //Si llega aca significa que el entorno a accesar no tiene parametros
        }

        private void ResetParametros(Entorno ent)
        {
            //Devuelve el estado de los parametros a uno actual
            foreach (Simbolo sim in ent.variables)
            {
                if (sim.TipoObjeto.Equals(Reservada.parametro))
                {
                    sim.Valor = getInicialDato(sim.Tipo);
                }
            }
        }


        private void ValidarParametrosMetodo(ParseTreeNode Nodo)
        {
            //ASIGNAR_PARAMETRO.Rule = ASIGNAR_PARAMETRO + coma + CONDICION
            //                         | CONDICION
            #region
            switch (Nodo.Term.Name)
            {
                case "ASIGNAR_PARAMETRO":
                    foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                    {
                        ValidarParametrosMetodo(hijo);
                    }
                    break;
                case "CONDICION":
                    Retorno retorno = Condicion(Nodo);
                    if (retorno != null)
                    {
                        arreglo.Add(new Celda(retorno.Tipo, retorno.Valor));
                    }
                    else
                    {
                        Console.WriteLine("Error Semantico-->Asignacion de parametro incorrecta linea:" + "0" + " columna:" + "0");
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion de parametro incorrecta", "0" + "", "0" + ""));
                    }
                    break;
            }
            #endregion
        }

        private string getCadenaPrint(ParseTreeNode Nodo)
        {
            switch(Nodo.Term.Name)
            {
                case "ASIGNAR_PARAMETRO":
                    switch(Nodo.ChildNodes.Count)
                    {
                        case 3:
                            return getCadenaPrint(Nodo.ChildNodes[0]) + getCadenaPrint(Nodo.ChildNodes[2]);
                            
                        case 1:
                            return getCadenaPrint(Nodo.ChildNodes[0]);
                            
                    }
                    break;
                case "CONDICION":
                    Retorno ret = Condicion(Nodo);
                    if (ret != null)
                    {
                        return ret.Valor + "";
                    }                        
                    return "";
                default:
                    return "";
            }
            return "";
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
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
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
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
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
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else if ((condC1.Tipo.Equals(Reservada.Cadena) && condC2.Tipo.Equals(Reservada.Cadena)))    //Si ambos son String
                            {
                                int v1 = getCantAscii(condC1.Valor);
                                int v2 = getCantAscii(condC2.Valor);

                                if (v1 <= v2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
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
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else if ((condD1.Tipo.Equals(Reservada.Cadena) && condD2.Tipo.Equals(Reservada.Cadena)))     //Si ambos son String
                            {
                                int v1 = getCantAscii(condD1.Valor);
                                int v2 = getCantAscii(condD2.Valor);

                                if (v1 >= v2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
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
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else if ((condE1.Tipo.Equals(Reservada.Cadena) && condE2.Tipo.Equals(Reservada.Cadena)))     //Si ambos son String
                            {
                                int v1 = getCantAscii(condE1.Valor);
                                int v2 = getCantAscii(condE2.Valor);

                                if (v1 < v2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
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
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno True
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno False
                                }
                            }
                            else if ((condF1.Tipo.Equals(Reservada.Cadena) && condF2.Tipo.Equals(Reservada.Cadena)))     //Si ambos son String
                            {
                                int v1 = getCantAscii(condF1.Valor);
                                int v2 = getCantAscii(condF2.Valor);

                                if (v1 > v2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
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
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else if ((condG1.Tipo.Equals(Reservada.Cadena) && condG2.Tipo.Equals(Reservada.Cadena)) ||      //Si ambos son String
                                    (condG1.Tipo.Equals(Reservada.Booleano) && condG2.Tipo.Equals(Reservada.Booleano)))     //Si ambos son Boolean
                            {
                                int v1 = getCantAscii(condG1.Valor);
                                int v2 = getCantAscii(condG2.Valor);

                                if (v1 == v2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
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
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno True
                                }
                            }
                            else if ((condH1.Tipo.Equals(Reservada.Cadena) && condH2.Tipo.Equals(Reservada.Cadena)) ||      //Si ambos son String
                                    (condH1.Tipo.Equals(Reservada.Booleano) && condH2.Tipo.Equals(Reservada.Booleano)))     //Si ambos son Boolean
                            {
                                int v1 = getCantAscii(condH1.Valor);
                                int v2 = getCantAscii(condH2.Valor);

                                if (v1 != v2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
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
                            return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])); //retorno False
                        }
                        else
                        {
                            return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])); //retorno True
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
                                return new Retorno(Reservada.Cadena, concat, linea1, colum1);
                            }
                            else if (ra1.Tipo.Equals(ra2.Tipo) && !ra1.Tipo.Equals(Reservada.Cadena)) // Si ambos son del mismo tipo y distinto de Cadena
                            {
                                double suma = double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);

                                if (ra1.Tipo.Equals(Reservada.Booleano))
                                {
                                    return new Retorno(Reservada.Booleano, suma + "", linea1, colum1);
                                }
                                else
                                {
                                    return new Retorno(ra1.Tipo, suma + "", linea1, colum1);
                                }
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real)) || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double suma = double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);
                                return new Retorno(Reservada.Real, suma + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero)) || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double suma = double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);
                                return new Retorno(Reservada.Entero, suma + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero)) || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real)))
                            {
                                double suma = double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);
                                return new Retorno(Reservada.Real, suma + "", linea1, colum1);
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
                                return new Retorno(Reservada.Real, resta + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Entero)) //Cualquier combinacion de estos valores da Entero
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double resta = double.Parse(GetOperable(ra1).Valor) - double.Parse(GetOperable(ra2).Valor);
                                return new Retorno(Reservada.Entero, resta + "", linea1, colum1);
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
                                return new Retorno(Reservada.Real, mul + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Entero))     //Cualquier combinacion de estos valores da Entero
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double mul = double.Parse(GetOperable(ra1).Valor) * double.Parse(GetOperable(ra2).Valor);
                                return new Retorno(Reservada.Entero, mul + "", linea1, colum1);
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
                                return new Retorno(Reservada.Real, div + "", linea1, colum1);
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
                                return new Retorno(Reservada.Real, mod + "", linea1, colum1);
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
                            return new Retorno(Reservada.Entero, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "real":
                            return new Retorno(Reservada.Real, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "cadena":
                            return new Retorno(Reservada.Cadena, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "true":
                            return new Retorno(Reservada.Booleano, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "false":
                            return new Retorno(Reservada.Booleano, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "id": //Esto puede ser una VARIABLE o un ARREGLO

                            #region
                            
                            String id = Nodo.ChildNodes[0].Token.Value.ToString();
                            Simbolo sim = RetornarSimbolo(id);

                            if (sim != null)
                            {
                                /*
                                if (sim.TipoObjeto.Equals(Reservada.arreglo))
                                {
                                    return new Retorno(Reservada.arreglo, id, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                }
                                else
                                {*/
                                    return new Retorno(sim.Tipo, sim.Valor, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                /*}*/
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
                            return new Retorno(Reservada.Entero, Nodo.ChildNodes[0].Term.Name + Nodo.ChildNodes[1].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                        case "real":
                            return new Retorno(Reservada.Real, Nodo.ChildNodes[0].Term.Name + Nodo.ChildNodes[1].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
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
                            
                            String id3 = Nodo.ChildNodes[0].Token.Value.ToString();
                            //Funciones func3 = tablafunciones.RetornarFuncion(id3);
                            Entorno func3 = getEntornoLocal(id3, cimaEnt.AmbitoPadre);

                            if (func3 != null)
                            {
                                int cantParams = 0;
                                foreach (Simbolo sim in func3.variables)
                                {
                                    if (sim.TipoObjeto.Equals(Reservada.parametro))
                                    {
                                        cantParams++;
                                    }
                                }

                                if(cantParams == 0)
                                {
                                    nivelActual++; //Aumentamos el nivel actual ya que accedemos a otro metodo
                                    TablaSimbolos metodo4 = new TablaSimbolos(nivelActual, Reservada.Funcion, true, false);
                                    pilaSimbolos.Push(metodo4);
                                    pilaEntornos.Push(func3);
                                    cimaTS = metodo4; //Estableciendo la tabla de simbolos cima
                                    cimaEnt = func3;
                                    cimaEnt.Activo = true;

                                    Retorno reto = EjecutarFuncion(cimaEnt.nodoBegin);

                                    cimaEnt.Activo = false;
                                    nivelActual--; //Disminuimos el nivel actual ya que salimos del metodo invocado
                                    pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                    pilaEntornos.Pop();
                                    cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                    cimaEnt = pilaEntornos.Peek();

                                    return reto;// ORIGINALMENTE DEVOLVER ESTO PERRO
                                }
                                else
                                {
                                    Console.WriteLine("Error Semantico-->Parametros incorrectos linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Parametros incorrectos", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Error Semantico-->Funcion no existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                            }
                            break;

                        case "(": //| parentA + CONDICION + parentC

                            Retorno ret = Condicion(Nodo.ChildNodes[1]);

                            if (ret != null)
                            {
                                return new Retorno(ret.Tipo, ret.Valor, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
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
                    switch (Nodo.ChildNodes[1].Term.Name)
                    {
                        case "(": // id + parentA + ASIGNAR_PARAMETRO + parentC // Invocacion funcion con parametros
                            //PRIMERO OBTENGO LA CANTIDAD DE VALORES EN MIS PARAMETROS ACEPTADOS POR EL ARREGLO
                            arreglo = new List<Celda>(); //Este arreglo es para almacenar los parametros del metodo que se invoco
                            ValidarParametrosMetodo(Nodo.ChildNodes[2]); // Mandamos los parametros
                                                                         //AHORA BUSCO LA FUNCION EN BASE AL NOMBRE Y A MI ARREGLO DE PARAMETROS
                            String id4 = Nodo.ChildNodes[0].Token.Value.ToString();

                            //Funciones func3 = tablafunciones.RetornarFuncion(id3);
                            Entorno func3 = getEntornoLocal(id4, cimaEnt.AmbitoPadre);

                            if (func3 != null)
                            {
                                func3 = EvaluarEnvioParametros(func3, arreglo);

                                if (func3 != null)
                                {
                                    nivelActual++; //Aumentamos el nivel actual ya que accedemos a otro metodo
                                    TablaSimbolos metodo4 = new TablaSimbolos(nivelActual, Reservada.Funcion, true, false);
                                    pilaSimbolos.Push(metodo4);
                                    pilaEntornos.Push(func3);
                                    cimaTS = metodo4; //Estableciendo la tabla de simbolos cima
                                    cimaEnt = func3;
                                    cimaEnt.Activo = true;

                                    Retorno reto = EjecutarFuncion(cimaEnt.nodoBegin);

                                    ResetParametros(cimaEnt);

                                    cimaEnt.Activo = false;
                                    nivelActual--; //Disminuimos el nivel actual ya que salimos del metodo invocado
                                    pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                    pilaEntornos.Pop();
                                    cimaTS = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                    cimaEnt = pilaEntornos.Peek();

                                    return reto;// ORIGINALMENTE DEVOLVER ESTO PERRO
                                }
                                else
                                {
                                    Console.WriteLine("Error Semantico-->Parametros de funcion no coinciden (2) linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Parametros de funcion no coinciden (2)", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Error Semantico-->Funcion no existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                            }
                            break;
                    }
                    break;

                default:
                    return null;

            }
            return null;
        }

        private Retorno EjecutarFuncion(ParseTreeNode cuerpo)
        {
            RetornoAc reta = Sentencias(cuerpo);

            if (reta.Retorna)
            {
                return new Retorno(reta.Tipo, reta.Valor, reta.Linea, reta.Columna);
            }
            return null;
        }

        private Simbolo RetornarSimbolo(String nombre)
        {
            if (cimaEnt.variables.Count != 0)
            {
                foreach (Simbolo simbol in cimaEnt.variables)
                {
                    if (simbol.Nombre.Equals(nombre))
                    {
                        return simbol;
                    }
                }
            }

            return RetornarSimboloAST(nombre, entorno);
        }

        private Simbolo RetornarSimboloAST(String nombre, List<Entorno> ent)
        {
            foreach (Entorno e in ent)
            {
                /*
                * Busco primero en los entornos que tiene el entorno actual, entornos hijos
                */
                if (e.Activo)
                {
                    if (e.entornos != null)
                    {
                        foreach (Entorno eAux in e.entornos)
                        {
                            if (eAux.Activo)
                            {
                                Simbolo sim = RetornarSimboloAST(nombre, eAux.entornos);
                                if (sim != null)
                                {
                                    return sim;
                                }
                            }                            
                        }
                    }

                    if (e.variables != null)
                    {
                        foreach (Simbolo sim in e.variables)
                        {
                            if (sim.Nombre.Equals(nombre))
                            {
                                return sim;
                            }
                        }
                    }
                }

            }
            return null;
        }

        private Entorno getEntornoLocal(string ambito, string ambitoPadre)
        {
            if (cimaEnt.entornos != null)
            {
                foreach (Entorno en in cimaEnt.entornos)
                {
                    if (en.Ambito.Equals(ambito))
                    {
                        //en.Activo = true;
                        return en;
                    }
                }
            }
            return getEntorno(ambito, ambitoPadre, entorno);
        }

        private Entorno getEntorno(string ambito, string ambitoPadre, List<Entorno> entorn)
        {
            foreach (Entorno ent in entorn)
            {
                /*
                 * Busco primero en los entornos que tiene el entorno actual, entornos hijos
                 */
                foreach (Entorno en in ent.Entornos)
                {
                    if (en.Ambito.Equals(ambito) && en.AmbitoPadre.Equals(ambitoPadre) && (en.Activo == false))
                    {
                        en.Activo = true;
                        return en;
                    }
                    Entorno ret = getEntorno(ambito, ambitoPadre, en.entornos);
                    if (ret != null)
                    {
                        return ret;
                    }
                }

                if (ent.Ambito.Equals(ambito) && ent.AmbitoPadre.Equals(ambitoPadre))
                {
                    ent.Activo = true;
                    return ent;
                }
            }
            return null;
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

        private string getLinea(ParseTreeNode Nodo)
        {
            try
            {
                return (Nodo.Token.Location.Line + 1) + "";
            }
            catch(Exception e)
            {
                return "fail";
            }
        }

        private string getColumna(ParseTreeNode Nodo)
        {
            try
            {
                return (Nodo.Token.Location.Column + 1) + "";
            }
            catch(Exception e)
            {
                return "fail";
            }
        }

        public List<Error> getErroresSemanticos()
        {
            return lstError;
        }
    }
}
