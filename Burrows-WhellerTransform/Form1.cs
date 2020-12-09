using System;
using System.Collections;
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
        private static Dictionary<(byte, byte),int>  XXCache = new Dictionary<(byte, byte), int>();
        private static Dictionary<int, int> SumOfICache = new Dictionary<int, int>();

        public OpenFileDialog openFileDialog1 { get; set; } = new OpenFileDialog();
        public SaveFileDialog SaveFileDialog { get; set; } = new SaveFileDialog();
        public static Dictionary<byte, double> AlphabetAndPropabilities { get; set; } = new Dictionary<byte, double>();
        public static List<byte> AlphabetSorted { get; set; } = new List<byte>();
        public static List<byte> ListToConvertByHuffman { get; set; } = new List<byte>();
        public static List<double> SortedPropabilities { get; set; } = new List<double>();
        public static List<byte> AlphabetLettersSortedByPropabilities { get; set; } = new List<byte>();
        public static List<List<byte>> C { get; set; }//матрица код слов
        public static List<byte> L { get; set; }//матрица длин код слов

        public Form1()
        {
            InitializeComponent();

            openFileDialog1.Filter = "All files(*.*)|*.*";
            SaveFileDialog.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            else
            {
                string filename = openFileDialog1.FileName;
                FileStream fileStream;

                List<byte> textAfterBurrowsWheller;
                FileInfo fileInfo;




                using (fileStream = File.OpenRead(filename))
                {
                    fileInfo = new FileInfo(filename);
                    textAfterBurrowsWheller = BurrowsWhellerTransformAllData(fileStream);
                }
                var textAfterMoveToFront = MoveToFrontCompressAllData(textAfterBurrowsWheller, GettingSortedAlphabet(textAfterBurrowsWheller));

                textAfterMoveToFront.InsertRange(0, BitConverter.GetBytes(AlphabetSorted.Count));//вставляем в начало размер алфавита

                textAfterMoveToFront.InsertRange(4, AlphabetSorted);//вставляем алфавит после количества символов в алфавите

                GettingAlphabetAndPropabilities(textAfterMoveToFront);//получаем упорядоченный алфавит в соответствии с вероятностями итогового сбщ и вероятности букв

                SortedPropabilities = AlphabetAndPropabilities.Select(d => d.Value).ToList();//сортируем вероятности для построения кода Хаффмана
                AlphabetLettersSortedByPropabilities = AlphabetAndPropabilities.Select(l => l.Key).ToList();


                HuffmanCodeBuilder(SortedPropabilities);//передаем список сортированых вероятностей из словаря


                //КОДИРОВАНИЕ ТЕКСТА
                var InformationAboutText = new List<byte>();//вся инфа о кодах Хаффмана для декодера

                InformationAboutText.Add((byte)(AlphabetAndPropabilities.Count - 1));//количество символов в алфавите
                //потому что мы отнимаем 1 при передаче, для того чтобы если алфавит 256, то число уместилось в 1 байт

                InformationAboutText.InsertRange(1, BitConverter.GetBytes(textAfterMoveToFront.Count));//кол-во символов в тексте

                var alphabetLetters = AlphabetAndPropabilities.Select(d => d.Key).ToList();
                InformationAboutText.InsertRange(5, alphabetLetters);//передаем алфавит в последовательносте убывания вероятностей каждой буквы

                InformationAboutText.InsertRange(5 + alphabetLetters.Count, L);//вставляем длины каждого слова Хаффмана к каждой букве

                List<byte> ListOfAllHuffmanWords = new List<byte>();//Все слова Хаффмана по убыванию вероятностей в списке
                for (int i = 0; i < C.Count; i++)
                {
                    ListOfAllHuffmanWords.AddRange(C[i]);
                }

                var codeText = HuffmanTextCoder(textAfterMoveToFront);

                ListOfAllHuffmanWords.AddRange(codeText);//склеиваем кодовые слова и текст из кодовых слов

                var wordsArrayAfterConvertion = ListOfAllHuffmanWords.ToBitArray(ListOfAllHuffmanWords.Count);
                var byteArrayWords = wordsArrayAfterConvertion.BitArrayToByteArray();//переработанный массив из слов алфавита и текста
                                                                                     

                InformationAboutText.AddRange(byteArrayWords);//полный текст на запись в файл

                using (FileStream fs = File.Create(fileInfo.DirectoryName + "\\" + $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}" + ".bwc"))
                {
                    fs.Write(InformationAboutText.ToArray(), 0, InformationAboutText.Count);
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            else
            {
                var OutputFileList = new List<byte>();//текст из сжатого файла
                string filename = openFileDialog1.FileName;
                FileStream fileStream;
                List<byte> HuffmanWordsWithData;
                List<byte> HuffmanDataInBitsList;
                FileInfo fileInfo;

                using (fileStream = File.OpenRead(filename))
                {
                    var fileSize = fileStream.Length;
                    byte[] buffer = new byte[fileSize];

                    fileInfo = new FileInfo(filename);
                    fileStream.Read(buffer, 0, Convert.ToInt32(fileSize));
                    OutputFileList = new List<byte>(buffer);

                }

                int alphabetSize = (int)OutputFileList[0];//размер алфавита
                alphabetSize += 1;//потому что мы отнимаем 1 при передаче, для того чтобы если алфавит 256, то число уместилось в 1 байт
                var userTextLenght = BitConverter.ToInt32(OutputFileList.GetRange(1, 4).ToArray(), 0);//количество букв в тексте

                var range = OutputFileList.GetRange(1, 4);
                var range2 = OutputFileList.GetRange(1, 5);

                AlphabetLettersSortedByPropabilities = new List<byte>(OutputFileList.GetRange(5, alphabetSize));//получаем алфавит в упорядоченном виде(по убыв вероятностей)
                L = new List<byte>(OutputFileList.GetRange((5 + alphabetSize), alphabetSize));//считываем длины код слов
                var header = alphabetSize * 2 + 5;//размер заголовка

                HuffmanWordsWithData = OutputFileList.GetRange(header, OutputFileList.Count - header);
                HuffmanDataInBitsList = HuffmanWordsWithData.ByteListToBitList();

                var textToDecode = GettingHuffmanWordsFromData(HuffmanDataInBitsList, alphabetSize, L);//кодовые слова вставляет в матрицу код слов, возвращает только текст для декодирования без алфавита в начале

                var textToMoveToFront = GettingDataFromHuffmanWords(textToDecode, userTextLenght);

                var textToBurrowsWheller = MoveToFrontDecompressAllData(textToMoveToFront);

                var textToWriteToFile = BurrowsWhellesDecompressAllData(textToBurrowsWheller);

                using (FileStream fs = File.Create(fileInfo.DirectoryName + "\\" + $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}"))
                {
                    fs.Write(textToWriteToFile.ToArray(), 0, textToWriteToFile.Count);
                }

            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            else
            {
                string filename = openFileDialog1.FileName;
                FileStream fileStream;
                List<byte> OutputFileList;
                List<double> SortedProp;
                List<byte> SortedAlphabet;

                using (fileStream = File.OpenRead(filename))
                {
                    var fileSize = fileStream.Length;
                    byte[] buffer = new byte[fileSize];

                    fileStream.Read(buffer, 0, Convert.ToInt32(fileSize));
                    OutputFileList = new List<byte>(buffer);
                }

                var dict = GettingAlphabetAndPropabilitiesForEntropy(OutputFileList);

                SortedProp = dict.Select(d => d.Value).ToList();
                SortedAlphabet = dict.Select(d => d.Key).ToList();

                var res = GettingHXEntropy(SortedProp);
                this.label1.Text = res.ToString();

                var propHXX = GettingHXXEntropy(OutputFileList, SortedAlphabet, SortedProp);
                this.label2.Text = propHXX.ToString();

                var propHXXX = GettingHXXXPropability(OutputFileList, SortedAlphabet, SortedProp);
                this.label3.Text = propHXXX.ToString();
            }
        }

        #region CoderAndDecoder
        public static List<byte> GettingDataFromHuffmanWords(List<byte> byteArray, int numberOfWords)
        {
            List<byte> ListToMoveToFront = new List<byte>();
            int iterator = 0;

            List<byte> newWord = new List<byte>();
            for (int i = 0; i < byteArray.Count; i++)
            {
                if (iterator != numberOfWords)
                {
                    newWord.Add(byteArray[i]);
                    for (int j = 0; j < C.Count; j++)
                    {
                        if (newWord.SequenceEqual(C[j]))
                        {
                            ListToMoveToFront.Add(AlphabetLettersSortedByPropabilities[j]);
                            iterator += 1;
                            newWord.Clear();
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }

            }

            return ListToMoveToFront;
        }

        public static List<byte> GettingHuffmanWordsFromData(List<byte> byteArray, int alphabetSize, List<byte> LenghtOfWords)
        {
            int index = 0;
            C = new List<List<byte>>();
            for (int i = 0; i < alphabetSize; i++)
            {
                C.Add(new List<byte>());
                C[i].AddRange(byteArray.GetRange(index, LenghtOfWords[i]));
                index += LenghtOfWords[i];
            }

            byteArray.RemoveRange(0, index);
            return byteArray;
        }

        public static List<byte> HuffmanTextCoder(List<byte> byteArray)//преобразует польз текст в текст Хаффмана
        {
            List<byte> outputList = new List<byte>();
            for (int i = 0; i < byteArray.Count; i++)
            {
                var c = AlphabetLettersSortedByPropabilities.IndexOf(byteArray[i]);
                var d = C[c];
                outputList.AddRange(d);
            }


            return outputList;
        }

        public static List<byte> BurrowsWhellerTransformAllData(Stream fileStream, int blockSize = 250)//blockSize - n-word
        {
            var numerator = 0;
            var fileSize = fileStream.Length;
            byte[] buffer = new byte[fileSize];
            var DataToBurrowsWhellerCompress = new List<byte>();
            List<byte> BufferAsList;
            uint numberOfBlocks = 0;

            fileStream.Read(buffer, 0, Convert.ToInt32(fileSize));
            BufferAsList = new List<byte>(buffer);

            while (numerator != -1)
            {
                if (numerator + blockSize <= fileSize)
                {
                    var sublist = BufferAsList.GetRange(numerator, blockSize);
                    DataToBurrowsWhellerCompress.AddRange(BurrowsWhellerCompressDataBlock(sublist));
                    numerator += blockSize;
                    numberOfBlocks += 1;//кол-во блоков до 4294967295, 4 байта
                }
                else
                {
                    var sublist = BufferAsList.GetRange(numerator, Convert.ToInt32(fileSize % blockSize));
                    if (sublist.Count != 0)
                    {
                        DataToBurrowsWhellerCompress.AddRange(BurrowsWhellerCompressDataBlock(sublist));
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
                if (i + 1 != numberOfBlocks)
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
            List<double> P = new List<double>(sortedProbabilities);//вероятности
            List<List<byte>> T = new List<List<byte>>();//потомки узлов
            C = new List<List<byte>>();//кодовые слова
            L = new List<byte>();//длины кодовых слов

            for (int i = 0; i < P.Count; i++)
            {
                T.Add(new List<byte>());//инициализация строки в матрице T
                C.Add(new List<byte>());//инициализация строки в матрице C
                L.Add(0);//инициализация строки в матрице (матрица одномерна)
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
                        L[item] += 1;
                    }
                    foreach (var item in T[p_jPosition])
                    {
                        T[p_iPosition].Add(item);
                        C[item].Add(1);
                        L[item] += 1;
                    }
                    T[p_jPosition].Clear();



                }
                else//меняем местами
                {
                    P[p_jPosition] = p_iValue + p_jValue;
                    foreach (var item in T[p_jPosition])
                    {
                        C[item].Add(0);
                        L[item] += 1;
                    }
                    foreach (var item in T[p_iPosition])
                    {
                        T[p_jPosition].Add(item);
                        C[item].Add(1);
                        L[item] += 1;
                    }
                    T[p_iPosition].Clear();
                }




            }

            for (int i = 0; i < C.Count; i++)
            {
                C[i].Reverse();
            }
            //реверс матрицы слов

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

        public static List<byte> MoveToFrontDecompressAllData(List<byte> byteArray)//block size - all word, слово с уже отделенным алфавитом
        {
            var numberOfSymbolsAlphabet = BitConverter.ToInt32(byteArray.GetRange(0, 4).ToArray(), 0);
            List<byte> OutputList = new List<byte>();
            List<byte> Alphabet = new List<byte>();

            Alphabet.AddRange(byteArray.GetRange(4, numberOfSymbolsAlphabet));
            byteArray.RemoveRange(0, 4 + Alphabet.Count);

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

            AlphabetSorted = AlphabetSorted.OrderBy(i => i).ToList();
            return AlphabetSorted;
        }

        public static void GettingAlphabetAndPropabilities(List<byte> byteArray/*List<byte> Alphabet, List<double> AlphabetPropabilities*/)
        {
            var numberOfSymbolsInArray = byteArray.Count;
            double countSymbols;//сколько раз встречается тот или иной элемент
            double propability;
            for (int i = 0; i < numberOfSymbolsInArray; i++)
            {
                if (AlphabetAndPropabilities.ContainsKey(byteArray[i]))
                {
                    continue;
                }
                else
                {
                    countSymbols = byteArray.Where(x => x.Equals(byteArray[i])).Count();
                    propability = countSymbols / numberOfSymbolsInArray;
                    AlphabetAndPropabilities.Add(byteArray[i], propability);
                }
            }


            AlphabetAndPropabilities = AlphabetAndPropabilities.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);//сортировка по вероятностям

        }

        #endregion

        #region Entropies 
        public static Dictionary<byte, double> GettingAlphabetAndPropabilitiesForEntropy(List<byte> byteArray/*List<byte> Alphabet, List<double> AlphabetPropabilities*/)
        {
            var numberOfSymbolsInArray = byteArray.Count;
            double countSymbols;//сколько раз встречается тот или иной элемент
            double propability;
            Dictionary<byte, double> AlphabetAndPropabilitiesEntropy = new Dictionary<byte, double>();

            for (int i = 0; i < numberOfSymbolsInArray; i++)
            {
                //string occur = "Test1";
                //IList<String> words = new List<string>() { "Test1", "Test2", "Test3", "Test1" };
                if (AlphabetAndPropabilitiesEntropy.ContainsKey(byteArray[i]))
                {
                    continue;
                }
                else
                {
                    countSymbols = byteArray.Where(x => x.Equals(byteArray[i])).Count();
                    propability = countSymbols / numberOfSymbolsInArray;
                    AlphabetAndPropabilitiesEntropy.Add(byteArray[i], propability);
                    //Alphabet.Add(byteArray[i]);
                    //AlphabetPropabilities.Add(propability);
                }
            }


             AlphabetAndPropabilitiesEntropy = AlphabetAndPropabilitiesEntropy.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);//сортировка по вероятностям

            return AlphabetAndPropabilitiesEntropy;
            //var count = AlphabetPropabilities.Sum(x => x);

        }

        public static double GettingHXEntropy(List<double> byteArray)
        {
            double res = 0;
            foreach (var item in byteArray)
            {
                res += -1 * item * Math.Log(item, 2);
            }
            return res;
        }

        private static int SumOfI(List<byte> byteArray, List<byte> AlphabetArray, int i)
        {
            if (SumOfICache.ContainsKey(i))
            {
                return SumOfICache[i];
            }

            SumOfICache[i] = byteArray.Where(x => x.Equals(AlphabetArray[i])).Count();
            return SumOfICache[i];
        }

      

        public static int GettingXXPropability(List<byte> byteArray, byte num1, byte num2)
        {
            if (XXCache.ContainsKey((num1, num2)))
            {
                return XXCache[(num1, num2)];
            }

            var res = 0;
            for (int i = 1; i < byteArray.Count; i++)
            {
                if(byteArray[i] == num1 && byteArray[i-1] == num2)
                {
                    res += 1;
                }
            }

            XXCache[(num1, num2)] = res;

            return res;
        }

        public static int GettingXXXPropability(List<byte> byteArray, byte num1, byte num2, byte num3)
        {
            var res = 0;
            for (int i = 2; i < byteArray.Count; i++)
            {
                if (byteArray[i] == num1 && byteArray[i - 1] == num2 && byteArray[i - 2] == num3)
                {
                    res += 1;
                }
            }

            return res;
        }

        public static double GettingHXXEntropy(List<byte> byteArray, List<byte> AlphabetArray, List<double> alphabetProp)
        {
            var res = 0.0;
            for (int i = 0; i < AlphabetArray.Count; i++)
            {
                int sumOfI = SumOfI(byteArray, AlphabetArray, i);
                for (int j = 0; j < AlphabetArray.Count; j++)
                {
                    var counter = GettingXXPropability(byteArray, AlphabetArray[j], AlphabetArray[i]);
                    var hx = 0.0;
                    if (counter == 0)
                        continue;
                   
                        var divRes = (double)counter / (double)sumOfI;
                        hx = -1 * divRes * Math.Log(divRes, 2) * alphabetProp[i];
                        res += hx;
                    
                }
            }


            return res;
        }

        public static double GettingHXXXPropability(List<byte> byteArray, List<byte> AlphabetArray, List<double> alphabetProp)
        {
            var res = 0.0;
            for (int i = 0; i < AlphabetArray.Count; i++)
            {
                int sumOfI = SumOfI(byteArray, AlphabetArray, i);
                for (int j = 0; j < AlphabetArray.Count; j++)
                {
                    var counterXX = GettingXXPropability(byteArray, AlphabetArray[j], AlphabetArray[i]);//количество комбинаций XX
                    //сколько раз XX встречается в тексте
                    if (counterXX == 0)
                        continue;

                    var propXX = (double)counterXX / (double)sumOfI;//Вероятность XX
                    for (int m = 0; m < AlphabetArray.Count; m++)
                    {
                        var counter = GettingXXXPropability(byteArray, AlphabetArray[m], AlphabetArray[j], AlphabetArray[i]);
                        //сколько раз XXX встреч в тексте
                        var hx = 0.0;
                        if (counter == 0)
                            continue;
                        
                            var divRes = (double)counter / (double)counterXX;//Вероятность X|XX
                            hx = -1 * divRes * Math.Log(divRes, 2) * propXX * alphabetProp[i];
                            res += hx;
                        
                    }
                }
            }


            return res;
        }
        #endregion
    }
}
