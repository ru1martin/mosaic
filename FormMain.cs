﻿
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace mosaic
{
    // Класс главной формы, размещает на себе элементы
    // управления - панель, прямоугольники PictureBox и другие.
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            
        }

        // Массив объектов прямоугольников для хранения сегментов картинки.
        PictureBox[] PB = null;
        // Длина стороны в прямоугольниках.
        int LengthSides = 3;
        // Объект хранения картинки.
        Bitmap Picture = null;

        // Создание области рисования картинки, для удобства определения ее размеров
        // прямоугольники массива PB размещаются на панели panel1.
        void CreatePictureRegion()
        {
            // Удалим предыдущий массив, чтобы создать новый.
            if (PB != null)
            {
                for (int i = 0; i < PB.Length; i++)
                {
                    PB[i].Dispose();
                }
                PB = null;
            }
            
            
            int num = LengthSides;
            // Создаем массив прямоугольников размером LengthSides Х LengthSides.
            PB = new PictureBox[num * num];
            
            // Вычислим габаритные размеры прямоугольников.
                int w = panel1.Width / num;
                int h = panel1.Height / num;

            int countX = 0; // счетчик прямоугольников по координате Х в одном ряду
            int countY = 0; // счетчик прямоуголников по координате Y в одном столбце
            for (int i = 0; i < PB.Length; i++)
            {
                PB[i] = new PictureBox(); // непосредственное создание прямоугольника, элемента массива

                // Размеры и координаты размещения созданного прямоугольника.
                PB[i].Width = w;
                PB[i].Height = h;
                PB[i].Left = 0 + countX * PB[i].Width;
                PB[i].Top = 0 + countY * PB[i].Height;

                // Запомним начальные координаты прямоугольника для
                // восстановления перемешанной картинки,
                // определения полной сборки картинки.
                Point pt = new Point();
                pt.X = PB[i].Left;
                pt.Y = PB[i].Top;
                PB[i].Tag = pt; // сохраним координаты в свойстве Tag каждого прямоугольника

                // Считаем прямоугольники по рядам и столбцам.
                countX++;
                if (countX == num)
                {
                    countX = 0;
                    countY++;
                }
                PB[i].Parent = panel1; // разместим прямоугольники на панели
                PB[i].BorderStyle = BorderStyle.None;
                PB[i].SizeMode = PictureBoxSizeMode.StretchImage; // размеры картинки будут подгоняться под размеры прямоугольника
                PB[i].Show(); // гарантия видимости прямоугольника

                // Для всех прямоугольников массива событие клика мыши
                // будет обрабатываться в одной и той же функции, для удобства
                // вычисления координат прямоугольников в одном месте.
                PB[i].Click += new EventHandler(PB_Click); 
            }

            // Раскидываем картинку на сегменты и рисуем каждый сегмент
            // на своем прямоугольнике.
            DrawPicture();
            
        }

        void DrawPicture()
        {
            if (Picture == null) return;
            int countX = 0;
            int countY = 0;
            int num = LengthSides;
            for (int i = 0; i < PB.Length; i++)
            {
                int w = Picture.Width / num;
                int h = Picture.Height / num;
                PB[i].Image = Picture.Clone(new RectangleF(countX * w, countY * h, w, h), Picture.PixelFormat);
                countX++;
                if (countX == LengthSides)
                {
                    countX = 0;
                    countY++;
                }

            }
        }

        void PB_Click(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)sender;

            
            for (int i = 0; i < PB.Length; i++)
            {
                // Сначала определим пустое место на области рисования картинки.
                if (PB[i].Visible == false)
                {
                    // Затем проверим выбранный прямоугольник и
                    // если у него совпадают координаты по X или Y,
                    // (откидываем из вычисления прямоугольники расположеннные по диагонали)
                    // но при этом он на ближайшем расстоянии от пустого
                    // прямоугольника
                    // (откидываем прямоугольники расположенные через прямоугольник от пустого)
                    if (
                        (pb.Location.X == PB[i].Location.X && 
                         Math.Abs(pb.Location.Y - PB[i].Location.Y) == PB[i].Height ) 
                        ||
                        (pb.Location.Y == PB[i].Location.Y && 
                         Math.Abs(pb.Location.X - PB[i].Location.X) == PB[i].Width)
                        )
                    {
                        // После успешной проверки
                        // меняем местами пустой и кликнутый прямоугольники.
                        Point pt = PB[i].Location;
                        PB[i].Location = pb.Location;
                        pb.Location = pt;

                        // После каждого хода проверка на полную сборку картинки.
                        //*************** блок проверки ***********************

                        // Если хоть у одного прямоугольника не совпадают
                        // реальные координаты и первичные выходим из функции.
                        for (int j = 0; j < PB.Length; j++)
                        {
                            Point point = (Point)PB[j].Tag;
                            if (PB[j].Location != point)
                            {
                                return;
                            }
                        }

                        // Если у всех прямоугольников совпали реальные и первичные 
                        // координаты - картинка собрана!
                        for (int m = 0; m < PB.Length; m++)
                        {
                            PB[m].Visible = true;
                            PB[m].BorderStyle = BorderStyle.None; // убираем обрамление прямоугольников
                        }
                        
                        //************** окончание блока проверки *************
                    }

                    break;
                }
            }

            

            
        }

        // Открытие диалогового окна выбора файла и создание новой области прорисовки картинки.
        private void toolStripButtonLoadPicture_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofDlg = new OpenFileDialog();
            // Фильтр показа только файлов с определенным расширением.
            ofDlg.Filter = "Файлы изображений (*.bmp;*.jpg;*.jpeg;)|*.bmp;*.jpg;.jpeg|All files (*.*)|*.*";
            ofDlg.FilterIndex = 1;
            ofDlg.RestoreDirectory = true;

            if (ofDlg.ShowDialog() == DialogResult.OK)
            {
                // Загружаем выбранную картинку.
                Picture = new Bitmap(ofDlg.FileName);
                // Создаем новую область прорисовки.
                CreatePictureRegion();
            }
        }

        // Перемешивание прямоугольников, хаотично меняем их координаты.
        private void toolStripButtonMixed_Click(object sender, EventArgs e)
        {
            if (Picture == null) return;

            // Создаем объект генерирования псевослучайных чисел
            Random rand = new Random(Environment.TickCount);
            int r = 0;
            for (int i = 0; i < PB.Length; i++)
            {
                PB[i].Visible = true;
                r = rand.Next(0, PB.Length);
                Point ptR = PB[r].Location;
                Point ptI = PB[i].Location;
                PB[i].Location = ptR;
                PB[r].Location = ptI;
                PB[i].BorderStyle = BorderStyle.FixedSingle;
            }

            // Случайным образом выбираем пустой прямоугольник,
            // делаем его невидимым.
            r = rand.Next(0, PB.Length);
            PB[r].Visible = false;
           
        }


        // В каждом прямоугольнике будет хранится соответствующий
        // сегмент картинки.
        

        // Восстанавливаем картинку соответсвенно первичным координатам.
        private void toolStripButtonRestore_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < PB.Length; i++)
            {
                Point pt = (Point)PB[i].Tag;
                PB[i].Location = pt;
                PB[i].Visible = true;
            }
        }

        // Открываем диалоговое окно настроек приложения.
        private void toolStripButtonSetting_Click(object sender, EventArgs e)
        {
            SetDlg setDlg = new SetDlg();
            setDlg.LengthSides = LengthSides;
            if (setDlg.ShowDialog() == DialogResult.OK)
            {
                LengthSides = setDlg.LengthSides;

                
                CreatePictureRegion();
            }
        }

        // Открываем диалоговое окно с нормальным видом картинки
        private void toolStripButtonHelp_Click(object sender, EventArgs e)
        {
            HelpDlg helpDlg = new HelpDlg();
            helpDlg.ImageDuplicate = Picture;
            helpDlg.ShowDialog();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            About About = new About();
       
            About.ShowDialog();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }



        
    }
}
