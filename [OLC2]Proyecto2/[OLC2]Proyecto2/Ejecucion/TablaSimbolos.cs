using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _OLC2_Proyecto2.Ejecucion
{
    class TablaSimbolos
    {
        public int Nivel;
        public String Tipo;
        public Boolean Retorna;
        public Boolean Detener;
        public List<Simbolo> ts = new List<Simbolo>();

        public TablaSimbolos(int Nivel, String Tipo, Boolean Retorna, Boolean Detener)
        {
            this.Nivel = Nivel;
            this.Tipo = Tipo;
            this.Retorna = Retorna;
            this.Detener = Detener;
        }

        public void addSimbolo(string ambito, string nombre, string valor, string tipo, string tipoobjeto, string linea, string columna, Boolean visibilidad, List<Celda> arreglo)
        {
            ts.Add(new Simbolo(ambito, nombre, valor, tipo, tipoobjeto, linea, columna, visibilidad, arreglo));
        }

        public List<Simbolo> getTS()
        {
            if (!vacio())
            {
                return ts;
            }
            return null;
        }

        public Boolean existeSimbolo(string nombre)
        {
            if (!vacio())
            {
                foreach (Simbolo simbolo in ts)
                {
                    if (simbolo.Nombre.Equals(nombre))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Boolean vacio()
        {
            if (!ts.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Simbolo RetornarSimbolo(String nombre)
        {
            if (!vacio())
            {
                foreach (Simbolo simbolo in ts)
                {
                    if (simbolo.Nombre.Equals(nombre))
                    {
                        return simbolo;
                    }
                }
            }
            return null;
        }

        public void LimpiarTS()
        {
            ts.Clear();
        }
    }
}
