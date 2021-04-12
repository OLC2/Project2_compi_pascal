using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;
using _OLC2_Proyecto2.Ejecucion;

namespace _OLC2_Proyecto2
{
    class Analizar : Grammar
    {

        public static Ejecutar ejecutar;

        public static List<TokenError> lista_errores = new List<TokenError>();

        //Para analizar
        public static ParseTree arbol = null;

        //Arbol
        private static ParseTreeNode raiz = null;

        public static Boolean esCadenaValida(string cadena)
        {
            raiz = null;

            Gramatica gramatica = new Gramatica();
            LanguageData language = new LanguageData(gramatica);
            Parser parser = new Parser(language);
            arbol = parser.Parse(cadena);
            raiz = arbol.Root;

            lista_errores.Clear();

            //================================================== PARA ERRORES LEXICOS Y SINTACTICOS ====================================================================
            int cont = 0;

            for (int i = 0; i < arbol.ParserMessages.Count(); i++)
            {
                if (arbol.ParserMessages.ElementAt(i).Message.Contains("Invalid character"))
                {
                    cont = 1;
                    TokenError newError = new TokenError();
                    newError = new TokenError(
                    arbol.ParserMessages.ElementAt(i).Location.Line,
                    arbol.ParserMessages.ElementAt(i).Location.Column,
                    "Error Lexico",
                    arbol.ParserMessages.ElementAt(i).Message.Replace("Invalid character:", "Caracter No Reconocido: ")
                    );
                    lista_errores.Add(newError);

                }

                else if (arbol.ParserMessages.ElementAt(i).Message.Contains("Syntax error"))
                {
                    if (!(arbol.ParserMessages.ElementAt(i).Message.Contains(",, $")))

                    {
                        cont = 1;
                        TokenError newError = new TokenError();
                        newError = new TokenError(
                        arbol.ParserMessages.ElementAt(i).Location.Line,
                        arbol.ParserMessages.ElementAt(i).Location.Column,
                        "Error Sintactico",
                        arbol.ParserMessages.ElementAt(i).Message.Replace("Syntax error, expected:", "Se esperaba: ")
                        );
                        lista_errores.Add(newError);
                    }
                }
            }
            //======================================================================================================================

            if (raiz == null)
            {
                return false;
            }

            return true;
        }

        public static void EjecutarAccionesPerronas()
        {
            ejecutar = new Ejecutar();
            ejecutar.lstPrint.Clear();
            ejecutar.lstError.Clear();

            
            try
            {
                if(ejecutar.pilaSimbolos != null) //******************************************************* DUDE
                {
                    ejecutar.pilaSimbolos.Clear();
                }                
                Console.WriteLine("Pila simbolos limpiada***");
            }
            catch (Exception e)
            {
                Console.WriteLine("Pila simbolos limpia***");
                Console.WriteLine("Error: "+e);
            }

            //ejecutar.IniciarEjecucion(raiz);//PRIMERA PASADA
            //RetornoAc retorno = ejecutar.EjecutarX(arregloX);//EJECUTA LAS ACCIONES DEL MAIN

            //return retorno;
            //return new RetornoAc("-", "-", "0", "0");
        }
    }
}
