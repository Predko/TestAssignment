# TestAssignment

Программа для работы с матрицами.
Разработана по тестовому заданию из
https://github.com/SyberryAcademy/Tasks-Library/blob/master/Matrix-Operations/matrix-operations.md

Выполняет операции сложения, отнимания, умножения с матрицами.

Допускает использование в качестве имени матрицы любой набор символов кроме пробелов, знаков табуляций. Допускаются знаки операций и символ начала матрицы, если они не являются первым символом имени.

Для выполнения действий с матрицами необязательно присвоение их переменным, можно их указывать непосредственно в выражении.

Допускаются строковые комментарии в стиле C++ (//)

Для вывода значения матрицы или результата операции, достаточно не присваивать результат.
В этом случае произойдёт вывод на экран(или в файл, если было перенаправление вывода).
Например файл 10.txt:

B12=[5 2 4; 0 2 -1; 3 -5 -4] // нет вывода.

E23=[-6 -5 -8; -1 -1 -10; 10 0 -7] // нет вывода.

Rddd=[-1 -7 6; -2 9 -4; 6 -10 2] // нет вывода.


Fqwwertreryeytre = Rddd+E23+B12 // нет вывода.

Fqwwertreryeytre -  [5 2 4; 0 2 -1; 3 -5 -4]  // Здесь будет выведен результат отнимания матрицы от переменной Fqwwertreryeytre.

Файл 10.out

[-7 -12 -2; -3 8 -14; 16 -10 -5]

