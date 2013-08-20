using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace AlgoResult
{
    public class Numero
    {
        public static int NBits(double delta, int precisao)
        {
            // mapeando o valor
            delta = Convert.ToInt32(delta * Math.Pow(10, precisao));
            return Convert.ToInt32(Math.Ceiling(Math.Log(delta, 2)));
        }

        public double ValorReal
        {
            get
            {
                // converte o valor em double
                return _valor / Math.Pow(10, _precisao);
            }
            set
            {
                // salvar o valor como inteiro, considerando a precisão
                double valor = value;
                valor *= Math.Pow(10, _precisao);
                _valor = Convert.ToInt32(valor);
            }
        }

        public List<bool> GetBits(double max, double min)
        {
            // função ceiling arrendonda o número para cima
            int nBits = NBits(max - min, _precisao);

            // valor normalizado
            int valNorm = Convert.ToInt32(_valor - min * Math.Pow(10, _precisao));

            // mapeando de acordo com o número de bits
            BitArray bits = new BitArray(BitConverter.GetBytes(valNorm));
            return bits.Cast<bool>().Take(nBits).ToList();
        }

        public void SetBits(List<bool> bits, double min, double max)
        {
            List<bool> bits32 = new List<bool>(32);

            for (int i = 0; i < 32; i++)
            {
                if (i < bits.Count)
                    bits32.Add(bits[i]);
                else bits32.Add(false);
            }

            BitArray bitArray = new BitArray(bits.ToArray());

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);

            // convertendo bits em inteiro
            _valor = array[0] + Convert.ToInt32(min * Math.Pow(10, _precisao));

            // mapeando o valor
            double deltaInt = Convert.ToInt32((max - min) * Math.Pow(10, _precisao));

            // operador de correção
            if (_valor > deltaInt)
            {
                double fora = _valor - deltaInt;
                // como se a dimensão inteira fosse circular:
                // depois do valor max, viria o valor minimo
                _valor = fora;
            }
        }

        private int _precisao;

        public Numero(int precisao)
        {
            _precisao = precisao;
        }

        // valor x 10 elevado a precisão
        private double _valor;
    }
}
