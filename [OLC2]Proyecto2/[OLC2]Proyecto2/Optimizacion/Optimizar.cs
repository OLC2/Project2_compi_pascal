using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;
using System.Diagnostics;
using System.Linq;
using _OLC2_Proyecto2.Ejecucion;

namespace _OLC2_Proyecto2.Optimizacion
{
    class Optimizar
    {
        public List<Error> lstError = new List<Error>();

        private Etiqueta etiquetas;

        public void iniciarOptimizacion(ParseTreeNode Nodo)
        {
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "S":
                        //S.Rule = ENCABEZADO + FUNCIONES
                        if (Nodo.ChildNodes.Count == 2)
                        {
                            Encabezado(Nodo.ChildNodes[0]);
                            Funciones(Nodo.ChildNodes[1]);
                        }
                        break;
                    default:
                        Debug.WriteLine("Error AST-->Nodo " + Nodo.Term.Name + " no existente/detectado");
                        break;
                }
            }
        }

        private void Encabezado(ParseTreeNode Nodo)
        {
            //ENCABEZADO.Rule = LIBRERIA + DECLARACIONES
            Debug.WriteLine("ENCABEZADO");
        }

        private void Funciones(ParseTreeNode Nodo)
        {
            /*
             * FUNCIONES.Rule = FUNCIONES + FUNCION
                            | FUNCION
             */
            try
            {
                switch (Nodo.Term.Name)
                {
                    case "FUNCIONES":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            Funciones(hijo); // SENTENCIA | SENTENCIAS
                        }
                        break;
                    case "FUNCION":
                        //FUNCION.Rule = ToTerm("void") + id + parentA + parentC + llaveA + ETIQUETAS + llaveC
                        #region 
                        etiquetas = new Etiqueta();
                        Debug.WriteLine("funcion: " + Nodo.ChildNodes[1].Token.Text);
                        SaveEtiquetas(Nodo.ChildNodes[5]);
                        #endregion
                        break;
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine("**Error Optimizacion (Funciones)");
            }
        }

        private void SaveEtiquetas(ParseTreeNode Nodo)
        {
            /*
             ETIQUETAS.Rule = ETIQUETAS + ETIQUETA
                            | ETIQUETA
            */
            try
            {
                switch (Nodo.Term.Name)
                {
                    case "ETIQUETAS":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            SaveEtiquetas(hijo); // SENTENCIA | SENTENCIAS
                        }
                        break;
                    case "ETIQUETA":
                        //ETIQUETA.Rule = etiqueta + dospuntos + SENTENCIAS
                        #region 
                        Debug.WriteLine("Etiqueta: " + Nodo.ChildNodes[0].Token.Text);
                        etiquetas.addEtiqueta(Nodo.ChildNodes[0].Token.Text);
                        Sentencias(Nodo.ChildNodes[2]);
                        #endregion
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("**Error Optimizacion (SaveEtiquetas)");
            }
        }

        private void Sentencias(ParseTreeNode Nodo)
        {
            //ETIQUETA.Rule = etiqueta + dospuntos + SENTENCIAS
            try
            {
                switch (Nodo.Term.Name)
                {
                    case "SENTENCIAS":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            Sentencias(hijo); // SENTENCIA | SENTENCIAS
                        }
                        break;
                    case "SENTENCIA":
                        #region
                        switch (Nodo.ChildNodes.Count)
                        {
                            case 2:
                                
                                break;
                            case 4:
                                                                
                                break;
                            case 5: 

                                break;
                            case 7:
                                //ToTerm("while") + CONDICION + ToTerm("do") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                                //ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                                
                                break;
                            case 11:
                                //ToTerm("for") + id + ToTerm(":=") + TERMINALES + ToTerm("to") + TERMINALES + ToTerm("do") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                                //ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + ToTerm("else") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                                
                                break;
                        }
                        #endregion
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("**Error Optimizacion (Sentencias)");
            }
        }
    }
}
