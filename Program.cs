using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TestAssignment
{
using static Console;    
    
    class Program
    {
        /// <summary>
        /// Словарь, для хранения переменных.
        /// </summary>
        static Dictionary<string, Matrix> Matrices = new();

        /// <summary>
        /// Стек значений и результатов операций.
        /// </summary>
        static Stack<Matrix> stackMatrices = new();
        
        /// <summary>
        /// Стек операций.
        /// </summary>
        static Stack<char> operations = new();

        static void Main(string[] args)
        {
            string[] lines = {"B=[5 2 4; 0 2 -1; 3 -5 -4]\r\n",
                              "E =[-6 -5 -8; -1 -1 -10; 10 0 -7]\r\n",
                              "R =[-1 -7 6; -2 9 -4; 6 -10 2]\r\n",
                              "\r\n",
                              "R + E * R * B + B\r\n"};

            int lineNumber = 0;

            MatrixReadErrors error;

            string nameVar = null;

            foreach (string input in lines)
            {
                string[] sa = GetTokens(input);

                lineNumber++;

                operations.Clear();
                stackMatrices.Clear();

                foreach (string token in sa)
                {
                    // Создание новой переменной или извлечение существующей. Переменная помещается в стек значений.
                    if (IsOperation(token) == false)
                    {
                        // Это переменная. Создаём(если нет) и помещаем в стек.
                        if (Matrices.ContainsKey(token) == false)
                        {
                            if (operations.Count == 0)
                            {
                                // Переменная не определена - создаём и помещаем в стек.
                                // Запоминаем имя переменной.
                                nameVar = token;

                                Matrices.Add(token, null);
                            }
                            else
                            {
                                // Попытка использовать неинициализированную переменную.
                                Error.WriteLine($"Exception caught: IllegalArgumentException. Can't read matrix. line: {lineNumber}");

                                return;
                            }
                        }
                        else
                        {
                            stackMatrices.Push(Matrices[token]);
                        }
                    }
                    // Если верно - операция присваивания, запоминаем в стеке операций.
                    else if (token[0] == '=')
                    {
                        operations.Push('=');
                    }
                    // Если верно, выполняем все предыдущие операции и помещаем результат в стек.
                    else if (token[0] == '\n')
                    {
                        while (operations.Count != 0)
                        {
                            if (operations.Peek() == '=')
                            {
                                // Выполняем операцию присвоения матрицы переменной.
                                operations.Pop();

                                Matrices[nameVar] = stackMatrices.Pop();

                                break;
                            }

                            char op = operations.Pop();

                            Matrix result = PerformOperation(op, lineNumber);

                            if (result == null)
                            {
                                Error.WriteLine($"Exception caught: IllegalArgumentException. Can't perform operation {op}. line: {lineNumber}");

                                return;
                            }

                            stackMatrices.Push(result);
                        }

                        if (stackMatrices.Count == 1)
                        {
                            // Это последний токен распечатываем результат.
                            WriteLine(stackMatrices.Pop().ToString());
                        }
                    }
                    // Если верно - поскольку приоритет операции ниже или равен приоритету предыдущей,
                    // выполняем все предыдущие операции, кроме присваивания.
                    else if (token[0] == '+' || token[0] == '-')
                    {
                        while (operations.Count != 0)
                        {
                            char op = operations.Pop();

                            if (op == '=')
                            {
                                // Операцию присваивания не выполняем.
                                break;
                            }

                            Matrix result = PerformOperation(op, lineNumber);

                            if (result == null)
                            {
                                Error.WriteLine($"Exception caught: IllegalArgumentException. Can't perform operation {op}. line: {lineNumber}");

                                return;
                            }

                            stackMatrices.Push(result);
                        }

                        operations.Push(token[0]);
                    }
                    // Если верно - выполняем предыдущую операцию умножения, если есть.
                    else if (token[0] == '*')
                    {
                        if (operations.Count != 0)
                        {
                            if (operations.Peek() == '*')
                            {
                                // Приоритет предыдущей операции равен приоритету текущей.
                                // Выполняем предыдущую операцию.
                                char op = operations.Pop();

                                Matrix result = PerformOperation(op, lineNumber);

                                if (result == null)
                                {
                                    Error.WriteLine($"Exception caught: IllegalArgumentException. Can't perform operation {op}. line: {lineNumber}");

                                    return;
                                }

                                stackMatrices.Push(result);
                            }
                        }

                        operations.Push(token[0]);
                    }
                    // Если верно - это определение матрицы. Извлекаем и создаём объект.
                    else if (token[0] == '[')
                    {
                        Matrix m;

                        if ((m = GetMatrix(token, out error)) == null)
                        {
                            Error.WriteLine("Exception caught: IllegalArgumentException. Can't read matrix.");

                            return;
                        }

                        Matrices[nameVar] = m;

                        stackMatrices.Push(m);
                    }
                }
            }
        }

        private static Matrix PerformOperation(char op, int lineNumber)
        {
            Matrix result;
            if (stackMatrices.Count < 2)
            {
                Error.WriteLine($"Exception caught: IllegalArgumentException. Can't perform operation '*'. line: {lineNumber}");
            }

            Matrix m1 = stackMatrices.Pop();
            Matrix m2 = stackMatrices.Pop();

            result = op switch
            {
                '*' => m1 * m2,
                '+' => m1 + m2,
                '-' => m1 - m2,
                  _ => null
            };

            return result;
        }

        /// <summary>
        /// Определяет, является ли строка знаком операции, присвоением,
        /// определением матрицы или признаком конца последовательности.
        /// </summary>
        /// <param name="token">Входной токен.</param>
        /// <returns>true - если это опрерация, знак присвоения или определения матрицы.</returns>
        private static bool IsOperation(string token)
        {
            return (token[0] == '=' || token[0] == '+' || token[0] == '-' || token[0] == '*' || token[0] == '[' || token[0] == '\n');
        }


        /// <summary>
        /// Разбивает строку на массив строк - токенов.
        /// </summary>
        /// <param name="input">Исходная строка.</param>
        /// <returns>Массив токенов.</returns>
        private static string[] GetTokens(string input)
        {
            List<string> strArr = new();

            int beginIndex = 0;

            int i = 0;

            while (i < input.Length)
            {
                switch (input[i])
                {
                    case '=':
                    case '+':
                    case '-':
                    case '*':
                        strArr.Add(input[beginIndex..i].Trim());
                        strArr.Add(input[i].ToString());

                        beginIndex = i + 1;

                        break;

                    case '[':

                        int endI = input.IndexOf(']', i + 1);

                        if (endI == -1)
                        {
                            Error.WriteLine("Exception caught: IllegalArgumentException. Can't read matrix.");

                            return null;
                        }

                        strArr.Add(input.Substring(beginIndex, endI - i + 1).Trim());

                        beginIndex = endI + 1;

                        i = endI;

                        break;

                    case '\r':
                    case '\n':

                        if (beginIndex != i)
                        {
                            strArr.Add(input[beginIndex..i].Trim());
                        }

                        // Переходим на конец строки.
                        i = input.Length - 1;

                        beginIndex = input.Length;

                        break;
                }

                i++;
            }

            if (beginIndex<input.Length)
            {
                strArr.Add(input[beginIndex..].Trim());

            }
            
            strArr.Add("\n");

            return strArr.ToArray();
        }

        /// <summary>
        /// Состояние ошибки чтения матрицы из строки.
        /// </summary>
        enum MatrixReadErrors { NotMatrix, NotInt, NumbColumns, ErrorNotFound }; 
        
        /// <summary>
        /// Определяет, является ли строка матрицей, если да читает её и создаёт объект типа Matrix.
        /// </summary>
        /// <param name="s">Входная строка.</param>
        /// <param name="error">Обнаруженные ошибки.</param>
        /// <returns></returns>
        private static Matrix GetMatrix(string s, out MatrixReadErrors error)
        {
            if (s.Length < 3 || s[0] != '[' || s[^1] != ']')
            {
                error = MatrixReadErrors.NotMatrix;

                return null;
            }

            string[] rowsMatrix = s.Trim('[', ']').Split(';');

            int rows = rowsMatrix.Length;
            int columns = rowsMatrix[0].Trim().Split(' ').Length;

            Matrix m = new(rows, columns);

            // for each row
            for (int r = 0; r != rows; r++)
            {
                string[] columnsMatrix = rowsMatrix[r].Trim().Split(' ');

                if (columnsMatrix.Length != columns)
                {
                    error = MatrixReadErrors.NumbColumns;

                    return null;
                }

                // for each column
                for (int c = 0; c != columnsMatrix.Length; c++)
                {
                    if (int.TryParse(columnsMatrix[c].Trim(), out int value) == false)
                    {
                        error = MatrixReadErrors.NotInt;

                        return null;
                    }

                    // Filling the matrix
                    m[r, c] = value;
                }
            }

            error = MatrixReadErrors.ErrorNotFound;

            return m;
        }

    }

    public class Vector: IEnumerable<int>
    {
        private readonly int[] values;

        public int Length { get => values.Length;  }

        public Vector(int[] v)
        {
            values = v;
        }

        public int this[int i]
        {
            get => values[i];
            set => values[i] = value;
        }

        public static int operator * (Vector v1, Vector v2)
        {
            int res = 0;

            for (int i = 0; i != v1.Length; i++)
            {
                res += v1[i] * v2[i];
            }

            return res;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return ((IEnumerable<int>)values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

    }
    
    
    public class Matrix
    {
        private readonly int[,] elements;

        private readonly int nRows;
        private readonly int nColumns;

        public int Rows { get => nRows; }
        public int Columns { get => nColumns; }

        public Matrix(int numbRows, int numbColumns)
        {
            nRows = numbRows;
            nColumns = numbColumns;

            elements = new int[nRows, nColumns];
        }

        public override string ToString()
        {
            StringBuilder s = new(200);

            s.Append('[');

            for (int r = 0; r != nRows; r++)
            {
                for (int c = 0; c != nColumns; c++)
                {
                    s.Append($"{elements[r, c]} ");
                }

                s.Replace(' ', ';', s.Length - 1, 1);
                
                s.Append(' ');
            }

            s.Append(']');

            return s.ToString();
        }

        /// <summary>
        /// Возвращает вектор колонку
        /// </summary>
        /// <param name="column">Номер вектора колонки.</param>
        /// <returns>Вектор или null если неправильный номер колонки.</returns>
        public Vector GetColumnVector(int column)
        {
            if (column < 0 || column >= Columns)
            {
                return null;
            }

            int[] vc = new int[Rows];

            for (int i = 0; i != Rows; i++)
            {
                vc[i] = elements[i, column];
            }

            return new Vector(vc);
        }

        /// <summary>
        /// Возвращает вектор сроку.
        /// </summary>
        /// <param name="row">Номер вектора строки.</param>
        /// <returns>Вектор или null если неправильный номер колонки.</returns>
        public Vector GetRowVector(int row)
        {
            if (row < 0 || row >= Rows)
            {
                return null;
            }

            int[] vc = new int[Columns];

            for (int i = 0; i != Rows; i++)
            {
                vc[i] = elements[row, i];
            }

            return new Vector(vc);
        }

        public int this[int i, int j]
        {
            get
            {
                return elements[i, j];
            }

            set
            {
                elements[i, j] = value;
            }
        }

        /// <summary>
        /// Сложение матриц.
        /// </summary>
        /// <param name="m1">Первая матрица.</param>
        /// <param name="m2">Вторая матрица.</param>
        /// <returns>Сумма матриц или null, если матрицы неодинаковые по размеру.</returns>
        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            return OperationAddOrSub(m1, m2, (a, b) => a + b);
        }

        /// <summary>
        /// Вычитание матриц.
        /// </summary>
        /// <param name="m1">Первая матрица.</param>
        /// <param name="m2">Вторая матрица.</param>
        /// <returns>Разность матриц или null, если матрицы неодинаковые по размеру.</returns>
        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            return OperationAddOrSub(m1, m2, (a, b) => a - b);
        }

        /// <summary>
        /// Операция умножения двух матриц.
        /// </summary>
        /// <param name="m1">Первая матрица.</param>
        /// <param name="m2">Вторая матрица.</param>
        /// <returns>Матрица - результат умножения или null, если обнаружена ошибка.</returns>
        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.Columns != m2.Rows)
            {
                return null;
            }

            Matrix res = new(m1.Rows, m2.Columns);

            for (int rm1 = 0; rm1 != m1.Rows; rm1++)
            {
                for (int cm2 = 0; cm2 != m2.Columns; cm2++)
                {
                    Vector v1 = m1.GetRowVector(rm1);
                    Vector v2 = m2.GetColumnVector(cm2);

                    if (v1 == null || v2 == null)
                    {
                        return null;
                    }

                    res[rm1, cm2] = v1 * v2;
                }
            }

            return res;
        }

        /// <summary>
        /// Выполняет операцию сложения или вычитания над матрицами в соответствии с передаваемой функцией.
        /// </summary>
        /// <param name="m1">Первая матрица.</param>
        /// <param name="m2">Вторая матрица.</param>
        /// <param name="operation">Выполняемая операция. Функция должна выполнять или сложение или вычитание.</param>
        /// <returns>Матрица - результат сложения или вычитания двух матриц.</returns>
        private static Matrix OperationAddOrSub(Matrix m1, Matrix m2, Func<int,int,int>operation)
        {
            if (m1.Rows != m2.Rows || m1.Columns != m2.Columns)
            {
                return null;
            }

            Matrix res = new(m1.Rows, m2.Columns);

            for (int r = 0; r != m1.Rows; r++)
            {
                for (int c = 0; c != m1.Columns; c++)
                {
                    res[r, c] = operation(m1[r, c], m2[r, c]);
                }
            }

            return res;
        }
    }
}
