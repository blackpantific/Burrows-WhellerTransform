using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Burrows_WhellerTransform
{
    public partial class Form1 : Form
    {
        public OpenFileDialog openFileDialog1 { get; set; } = new OpenFileDialog();
        public static List<byte> Alphabet { get; set; } = new List<byte>();
        public static List<double> AlphabetPropabilities { get; set; } = new List<double>();
        public static List<byte> AlphabetSorted { get; set; } = new List<byte>();
        public static List<byte> ListToConvertByHuffman { get; set; } = new List<byte>();

        public Form1()
        {
            InitializeComponent();

            button1.Click += button1_Click;
            openFileDialog1.Filter = "All files(*.*)|*.*";

            List<byte> listByte = new List<byte>();
            listByte.Add(14);
            listByte.Add(9);
            listByte.Add(5);
            listByte.Add(21);
            listByte.Add(32);

            List<byte> alphabet = new List<byte>();
            alphabet.Add(5);
            alphabet.Add(9);
            alphabet.Add(14);
            alphabet.Add(21);
            alphabet.Add(32);

            List<double> propabilities = new List<double>();
            propabilities.Add(0.4);
            propabilities.Add(0.25);
            propabilities.Add(0.08);
            propabilities.Add(0.07);
            propabilities.Add(0.09);
            propabilities.Add(0.06);
            propabilities.Add(0.05);

            //var res = BurrowsWhellerCompressDataBlock(listByte);
            //BurrowsWhellerDecompressDataBlock(res);

            //var list = MoveToFrontCompressAllData(listByte, alphabet);
            //MoveToFrontDecompressAllData(list, alphabet);

            //HuffmanCodeBuilder(propabilities);

            

            /////////////////////////////////////////////////////////////////////
            //var index = Alphabet.IndexOf(21);                                     можно проверять наличие буквы в алфавите
            /////////////////////////////////////////////////////////////////////
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            string filename = openFileDialog1.FileName;
            FileStream fileStream;

            using (fileStream = File.OpenRead(filename))
            {
                //Alphabet.Add(5);
                //Alphabet.Add(48);
                //Alphabet.Add(49);
                //Alphabet.Add(50);
                //Alphabet.Add(51);
                //Alphabet.Add(52);
                //Alphabet.Add(53);
                //Alphabet.Add(54);
                //Alphabet.Add(55);
                //Alphabet.Add(56);
                //Alphabet.Add(57);

                var a = BurrowsWhellerTransformAllData(fileStream);

                var b = MoveToFrontCompressAllData(a, GettingSortedAlphabet(a));

                var numSymbolsInAlphabet = AlphabetSorted.Count;

                

                //var c = MoveToFrontDecompressAllData(b, AlphabetSorted);

                //ВАЖНО ВАЖНО ВАЖНО ВАЖНО ВАЖНО ВАЖНО ВАЖНО ВАЖНО ВАЖНО ВАЖНО ВАЖНО ВАЖНО ВАЖНО!!!

                //ToInt32(Byte[], Int32)                           //////////////////////////////////////////////////////////
                //uint a = BitConverter.ToUInt32(BitConverter.GetBytes(numberOfBlocks), 0);
            }



        }

        public static List<byte> BurrowsWhellerTransformAllData(Stream fileStream, int blockSize = 250)//blockSize - n-word
        {
            var numerator = 0;
            var fileSize = fileStream.Length;
            byte[] buffer = new byte[fileSize];
            List<byte> DataToBurrowsWhellerCompress = new List<byte>();//??
            List<byte> BufferAsList;
            uint numberOfBlocks = 0;

            /////////////////СОЗДАТЬ АЛФАВИТ ДЛЯ КОДА ХАФФМАНА

            fileStream.Read(buffer, 0, Convert.ToInt32(fileSize));
            BufferAsList = new List<byte>(buffer);

            while (numerator != -1)
            {
                if (numerator + blockSize <= fileSize)
                {
                    var sublist = BufferAsList.GetRange(numerator, blockSize);
                    DataToBurrowsWhellerCompress.AddRange(BurrowsWhellerCompressDataBlock(sublist));

                    //var a = BurrowsWhellerDecompressDataBlock(BurrowsWhellerCompressDataBlock(sublist));

                    numerator += blockSize;
                    numberOfBlocks += 1;//кол-во блоков до 4294967295, 4 байта
                }
                else
                {
                    var sublist = BufferAsList.GetRange(numerator, Convert.ToInt32(fileSize % blockSize));
                    if (sublist.Count != 0)
                    {
                        DataToBurrowsWhellerCompress.AddRange(BurrowsWhellerCompressDataBlock(sublist));

                        //var a = BurrowsWhellerDecompressDataBlock(BurrowsWhellerCompressDataBlock(sublist));

                        numberOfBlocks += 1;
                    }
                    break;
                }
            }

            DataToBurrowsWhellerCompress.InsertRange(0, BitConverter.GetBytes(numberOfBlocks));//вставляем на 0 позицию 4 бита, те кол-во слов

            return DataToBurrowsWhellerCompress;
        }

        public static List<byte> BurrowsWhellesDecompressAllData(List<byte> byteArray)
        {
            var numerator = 4;//первые 4 байта под количество блоков отведены
            var listSize = byteArray.Count;
            List<byte> DataToWriteToFile = new List<byte>();
            var numberOfBlocks = BitConverter.ToUInt32(byteArray.GetRange(0, 4).ToArray(), 0);

            for (int i = 0; i < numberOfBlocks; i++)
            {
                if(i + 1 != numberOfBlocks)
                {
                    var subset = byteArray.GetRange(numerator, 251);//251 или 6
                    DataToWriteToFile.AddRange(BurrowsWhellerDecompressDataBlock(subset));
                    numerator += 251;
                }
                else
                {
                    var subset = byteArray.GetRange(numerator, listSize - numerator);
                    DataToWriteToFile.AddRange(BurrowsWhellerDecompressDataBlock(subset));
                }
            }

            return DataToWriteToFile;

        }

        public static void HuffmanCodeBuilder(List<double> sortedProbabilities)//получает отсорт вер-ти и выдает готовые коды на каждый символ
        {
            List<double> P = new List<double>(sortedProbabilities);
            List<List<byte>> T = new List<List<byte>>();
            List<List<byte>> C = new List<List<byte>>();

            for (int i = 0; i < P.Count; i++)
            {
                T.Add(new List<byte>());//инициализация строки в матрице T
                C.Add(new List<byte>());//инициализация строки в матрице C
                T[i].Add(Convert.ToByte(i));
            }

            bool endBuilding = true;//пока в матрице P не останется единственный элемент 
            Func<double, bool> func = (arg) => arg != 0;
            double p_iValue = 0;
            double p_jValue = 0;
            int p_iPosition = 0;
            int p_jPosition = 0;
            while (endBuilding)
            {
                


                p_iValue = P.Where(func).Min();
                p_iPosition = P.IndexOf(p_iValue);
                P[p_iPosition] = 0;

                try
                {
                    p_jValue = P.Where(func).Min();
                    p_jPosition = P.IndexOf(p_jValue);
                    P[p_jPosition] = 0;
                }
                catch                //мы закончили создание код слов, так как остался 1 элемент и все отортированы
                {
                    endBuilding = false;
                    break;
                }
                if (p_iPosition < p_jPosition)//все ок, по алгоритму 
                {
                    P[p_iPosition] = p_iValue + p_jValue;
                    foreach (var item in T[p_iPosition])
                    {
                        C[item].Add(0);
                    }
                    foreach (var item in T[p_jPosition])
                    {
                        T[p_iPosition].Add(item);
                        C[item].Add(1);
                    }
                    T[p_jPosition].Clear();

                    

                }
                else//меняем местами
                {
                    P[p_jPosition] = p_iValue + p_jValue;
                    foreach (var item in T[p_jPosition])
                    {
                        C[item].Add(0);
                    }
                    foreach (var item in T[p_iPosition])
                    {
                        T[p_jPosition].Add(item);
                        C[item].Add(1);
                    }
                    T[p_iPosition].Clear();
                }

                


            }

        }

        public static List<byte> MoveToFrontCompressAllData(List<byte> byteArray, List<byte> alphabet)//block size - all word, алфавит уже упорядоченный
        {
            List<byte> OutputList = new List<byte>();
            List<byte> Alphabet = new List<byte>(alphabet);
            for (int i = 0; i < byteArray.Count; i++)//цикл по входному списку
            {
                for (int j = 0; j < Alphabet.Count; j++)//цикл по алфавиту
                {
                    if (byteArray[i] == Alphabet[j])
                    {
                        OutputList.Add(Convert.ToByte(j));
                        Alphabet.Insert(0, Alphabet[j]);
                        Alphabet.RemoveAt(j + 1);
                        break;
                    }
                }
            }

            return OutputList;
        }

        public static List<byte> MoveToFrontDecompressAllData(List<byte> byteArray, List<byte> alphabet)//block size - all word, слово с уже отделенным алфавитом
        {
            List<byte> OutputList = new List<byte>();
            List<byte> Alphabet = new List<byte>(alphabet);
            for (int i = 0; i < byteArray.Count; i++)//цикл по входному списку
            {
                for (int j = 0; j < Alphabet.Count; j++)//цикл по алфавиту
                {
                    if (byteArray[i] == j)
                    {
                        OutputList.Add(Alphabet[j]);//можно объединить 2 метода, но стоит вопрос о производительности
                        Alphabet.Insert(0, Alphabet[j]);
                        Alphabet.RemoveAt(j + 1);
                        break;
                    }
                }

            }

            return OutputList;

        }

        public static List<byte> BurrowsWhellerCompressDataBlock(List<byte> byteArray)//blockSize - n-word
        {
            List<List<byte>> ShiftMatrix = new List<List<byte>>();
            ShiftMatrix.Add(byteArray);
            var newByteArrayWithShift = new List<byte>(byteArray);
            var lastIndexElement = newByteArrayWithShift[byteArray.Count - 1];
            for (int i = 0; i < byteArray.Count - 1; i++)//получаем все строки матрицы со смещением
            {
                newByteArrayWithShift.Insert(0, lastIndexElement);
                newByteArrayWithShift.RemoveAt(byteArray.Count);
                ShiftMatrix.Add(new List<byte>(newByteArrayWithShift));
                lastIndexElement = newByteArrayWithShift[byteArray.Count - 1];
            }

            var sortedList = ShiftMatrix.OrderBy(l => l, new ListComparer<byte>()).ToList();//сортировка строк матрицы и создание новой

            var positionOfByteArrayInMatrix = Convert.ToByte(0);// размер блока, на который разбивается
            //файл не должен превышать 255

            for (int i = 0; i < sortedList.Count; i++)
            {
                if (byteArray.SequenceEqual(sortedList[i]))
                {
                    positionOfByteArrayInMatrix = Convert.ToByte(i + 1);
                    break;
                }
            }

            List<byte> outputList = new List<byte>();
            for (int i = 0; i < sortedList.Count; i++)
            {
                outputList.Add(sortedList[i][sortedList.Count - 1]);
            }

            outputList.Add(positionOfByteArrayInMatrix);
            return outputList;
        }

        public static List<byte> BurrowsWhellerDecompressDataBlock(List<byte> byteArray)//blockSize - n-word
        {
            var positionOfByteArrayInMatrix = byteArray[byteArray.Count - 1];
            byteArray.RemoveAt(byteArray.Count - 1);
            List<List<byte>> ResultMatrix = new List<List<byte>>();
            for (int i = 0; i < byteArray.Count; i++)//инициализация матрицы, 1 шаг
            {
                ResultMatrix.Add(new List<byte>());
            }

            for (int i = 0; i < byteArray.Count; i++)//добавление столбцов и их сортировка
            {
                for (int j = 0; j < byteArray.Count; j++)
                {
                    ResultMatrix[j].Insert(0, byteArray[j]);
                }
                ResultMatrix = ResultMatrix.OrderBy(l => l, new ListComparer<byte>()).ToList();
            }

            return ResultMatrix[positionOfByteArrayInMatrix - 1];
        }

        public static List<byte> GettingSortedAlphabet(List<byte> byteArray)
        {
            for (int i = 0; i < byteArray.Count; i++)
            {
                if (AlphabetSorted.Contains(byteArray[i]))
                {
                    continue;
                }
                else
                {
                    AlphabetSorted.Add(byteArray[i]);
                }
            }

            AlphabetSorted = AlphabetSorted.OrderBy(i =>i).ToList();
            return AlphabetSorted;
        }


    }
}
