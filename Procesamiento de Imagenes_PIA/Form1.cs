using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms.DataVisualization.Charting;
using System.Runtime.InteropServices;

namespace Procesamiento_de_Imagenes_PIA
{

    public partial class Form1 : Form
    {


        private bool isHovered = false;
        private bool isClicked = false;
        private Button activeButton = null;

       

        // BOTONES
        // 🔹 Evento: Agrega la línea blanca al hacer clic
        private void AppleButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;

            // Si el botón ya está activo, no cambiar nada
            if (activeButton == clickedButton)
                return;

            // Cambiar la "actividad" de los botones
            if (activeButton != null)
            {
                // Si ya hay un botón activo, limpiamos la línea
                activeButton.Invalidate();
            }

            activeButton = clickedButton; // Establecer el nuevo botón como activo
            activeButton.Invalidate(); // Redibujar el botón activo
        }

        // 🔹 Evento: Dibuja la línea blanca cuando se hace clic
        private void AppleButton_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Button btn = (Button)sender;

            // Brillo en el texto cuando se pasa el mouse
            btn.ForeColor = isHovered ? Color.White : Color.White;

            // Solo dibujar la línea en el botón activo
            if (btn == activeButton)
            {
                using (Pen pen = new Pen(Color.White, 2))
                {
                    g.DrawLine(pen, 10, btn.Height - 5, btn.Width - 10, btn.Height - 5);
                }
            }





        }

        public Form1()
        {
            InitializeComponent();
            AddAppleButtons();
            ConfigurarChart();


            int cornerRadius = 40; // Aumentado para más curvas

            // Crear un GraphicsPath para las esquinas redondeadas
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90); // Esquina superior izquierda
            path.AddArc(this.Width - cornerRadius - 1, 0, cornerRadius, cornerRadius, 270, 90); // Esquina superior derecha
            path.AddArc(this.Width - cornerRadius - 1, this.Height - cornerRadius - 1, cornerRadius, cornerRadius, 0, 90); // Esquina inferior derecha
            path.AddArc(0, this.Height - cornerRadius - 1, cornerRadius, cornerRadius, 90, 90); // Esquina inferior izquierda
            path.CloseAllFigures(); // Cerrar el camino para que las líneas sean continuas

            // Asignar la forma redondeada al formulario
            this.Region = new Region(path);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // BOTONES
            button1.Click += AppleButton_Click;
            button1.Paint += AppleButton_Paint;
            button2.Click += AppleButton_Click;
            button2.Paint += AppleButton_Paint;
            button3.Click += AppleButton_Click;
            button3.Paint += AppleButton_Paint;
        }

        private void AddAppleButtons()
        {
            Button closeButton = CreateButton(Color.Tomato, new Point(15, 10));
            Button minimizeButton = CreateButton(Color.Gold, new Point(40, 10));
            Button maximizeButton = CreateButton(Color.YellowGreen, new Point(65, 10));

            closeButton.Click += (s, e) => this.Close();
            minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            maximizeButton.Click += (s, e) =>
            {
                this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            };

            this.Controls.Add(closeButton);
            this.Controls.Add(minimizeButton);
            this.Controls.Add(maximizeButton);
        }

        private Button CreateButton(Color color, Point location)
        {
            Button button = new Button
            {
                Size = new Size(15, 15),
                Location = location,
                BackColor = color,
                FlatStyle = FlatStyle.Flat
            };
            button.FlatAppearance.BorderSize = 0;
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, button.Width, button.Height);
            button.Region = new Region(path);
            return button;
        }
        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }


        //cambio de pantallas
        private void button1_Click(object sender, EventArgs e)
        {
            Form1 nuevoForm = new Form1();

            nuevoForm.StartPosition = FormStartPosition.Manual;
            nuevoForm.Location = this.Location;
            nuevoForm.Show();
            this.Hide();
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            Form4 nuevoForm = new Form4();

            nuevoForm.StartPosition = FormStartPosition.Manual;
            nuevoForm.Location = this.Location;
            nuevoForm.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form5 nuevoForm = new Form5();

            nuevoForm.StartPosition = FormStartPosition.Manual;
            nuevoForm.Location = this.Location;
            nuevoForm.Show();
            this.Hide();
        }





        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar imagen",
                Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox4.Image = new Bitmap(openFileDialog.FileName);
                
            }
        }



        private void label4_Click(object sender, EventArgs e)
        {
            if (pictureBox4.Image != null)
            {
                Bitmap imagen = new Bitmap(pictureBox4.Image);

                for (int y = 0; y < imagen.Height; y++)
                {
                    for (int x = 0; x < imagen.Width; x++)
                    {
                        Color pixel = imagen.GetPixel(x, y);
                        float h, s, v;
                        RGBtoHSV(pixel, out h, out s, out v);
                        h = (h + 30) % 360; // Cambiar el matiz (ajustable)
                        imagen.SetPixel(x, y, HSVtoRGB(h, s, v));
                    }
                }
                pictureBox5.Image = imagen;
                ConfigurarChart();
            }
            else
            {
                MessageBox.Show("Primero selecciona una imagen.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {
            if (pictureBox4.Image != null)
            {
                Bitmap imagen = new Bitmap(pictureBox4.Image);
                Bitmap imagenPintura = new Bitmap(imagen.Width, imagen.Height);

                // Definir el filtro de Sobel para bordes
                int[,] filtroSobelX = new int[,]
                {
            {-1, 0, 1},
            {-2, 0, 2},
            {-1, 0, 1}
                };

                int[,] filtroSobelY = new int[,]
                {
            {-1, -2, -1},
            { 0,  0,  0},
            { 1,  2,  1}
                };

                for (int y = 1; y < imagen.Height - 1; y++)
                {
                    for (int x = 1; x < imagen.Width - 1; x++)
                    {
                        int rX = 0, gX = 0, bX = 0;
                        int rY = 0, gY = 0, bY = 0;

                        // Aplicando filtro Sobel X
                        for (int j = -1; j <= 1; j++)
                        {
                            for (int i = -1; i <= 1; i++)
                            {
                                Color pixel = imagen.GetPixel(x + i, y + j);
                                rX += pixel.R * filtroSobelX[j + 1, i + 1];
                                gX += pixel.G * filtroSobelX[j + 1, i + 1];
                                bX += pixel.B * filtroSobelX[j + 1, i + 1];
                                rY += pixel.R * filtroSobelY[j + 1, i + 1];
                                gY += pixel.G * filtroSobelY[j + 1, i + 1];
                                bY += pixel.B * filtroSobelY[j + 1, i + 1];
                            }
                        }

                        // Calcular la magnitud del gradiente en X y Y
                        int r = (int)Math.Min(Math.Sqrt(rX * rX + rY * rY), 255);
                        int g = (int)Math.Min(Math.Sqrt(gX * gX + gY * gY), 255);
                        int b = (int)Math.Min(Math.Sqrt(bX * bX + bY * bY), 255);

                        imagenPintura.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
                pictureBox5.Image = imagenPintura;
                ConfigurarChart();
            }
            else
            {
                MessageBox.Show("Primero selecciona una imagen.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
            if (pictureBox4.Image != null)
            {
                Bitmap imagen = new Bitmap(pictureBox4.Image);

                for (int y = 0; y < imagen.Height; y++)
                {
                    for (int x = 0; x < imagen.Width; x++)
                    {
                        Color pixel = imagen.GetPixel(x, y);
                        float h, s, v;
                        RGBtoHSV(pixel, out h, out s, out v);
                        s = Math.Min(s * 2.5f, 1); // Aumenta la saturación
                        imagen.SetPixel(x, y, HSVtoRGB(h, s, v));
                    }
                }
                pictureBox5.Image = imagen;
                ConfigurarChart();
            }
            else
            {
                MessageBox.Show("Primero selecciona una imagen.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
            if (pictureBox4.Image != null)
            {
                Bitmap imagen = new Bitmap(pictureBox4.Image);
                int niveles = 4, paso = 255 / (niveles - 1);

                for (int y = 0; y < imagen.Height; y++)
                {
                    for (int x = 0; x < imagen.Width; x++)
                    {
                        Color p = imagen.GetPixel(x, y);
                        imagen.SetPixel(x, y, Color.FromArgb((p.R / paso) * paso, (p.G / paso) * paso, (p.B / paso) * paso));
                    }
                }
                pictureBox5.Image = imagen;
                ConfigurarChart();
            }
            else
            {
                MessageBox.Show("Primero selecciona una imagen.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RGBtoHSV(Color color, out float h, out float s, out float v)
        {
            float r = color.R / 255f, g = color.G / 255f, b = color.B / 255f;
            float max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b)), delta = max - min;
            h = delta == 0 ? 0 : (max == r ? (g - b) / delta + (g < b ? 6 : 0) : max == g ? (b - r) / delta + 2 : (r - g) / delta + 4) * 60;
            s = max == 0 ? 0 : delta / max; v = max;
        }

        private Color HSVtoRGB(float h, float s, float v)
        {
            float c = v * s, x = c * (1 - Math.Abs((h / 60) % 2 - 1)), m = v - c;
            float r = 0, g = 0, b = 0;
            if (h < 60) { r = c; g = x; }
            else if (h < 120) { r = x; g = c; }
            else if (h < 180) { g = c; b = x; }
            else if (h < 240) { g = x; b = c; }
            else if (h < 300) { r = x; b = c; }
            else { r = c; b = x; }
            return Color.FromArgb((int)((r + m) * 255), (int)((g + m) * 255), (int)((b + m) * 255));
        }

        private void button4_Click(object sender, EventArgs e)
        {
                    }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {
            if (pictureBox5.Image == null)
            {
                MessageBox.Show("No hay una imagen con filtro para guardar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Guardar imagen con filtro";
                saveFileDialog.Filter = "Imagen PNG|*.png|Imagen JPEG|*.jpg|Bitmap|*.bmp";
                saveFileDialog.DefaultExt = "png";
                saveFileDialog.FileName = "tu mmda";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Guardamos la imagen mostrada en pictureBox5
                        pictureBox5.Image.Save(saveFileDialog.FileName);
                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al guardar la imagen: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(pictureBox4.Image);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            IntPtr ptr = data.Scan0;
            int bytes = Math.Abs(stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            for (int i = 0; i < bytes; i++)
            {
                rgbValues[i] = (byte)((rgbValues[i] < 128) ? 255 - rgbValues[i] : rgbValues[i]);
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            bmp.UnlockBits(data);
            pictureBox5.Image = bmp;
            ConfigurarChart();
        }

        private void label11_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(pictureBox4.Image);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            IntPtr ptr = data.Scan0;
            int bytes = Math.Abs(stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            for (int i = 0; i < bytes; i += 3)
            {
                float r = rgbValues[i + 2];
                float g = rgbValues[i + 1];
                float b = rgbValues[i];

                float gray = (r + g + b) / 3f;
                rgbValues[i + 2] = (byte)Math.Min(255, gray + (r - gray) * 3);
                rgbValues[i + 1] = (byte)Math.Min(255, gray + (g - gray) * 3);
                rgbValues[i] = (byte)Math.Min(255, gray + (b - gray) * 3);
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            bmp.UnlockBits(data);
            pictureBox5.Image = bmp;
            ConfigurarChart();
        }

        private void label12_Click(object sender, EventArgs e)
        {
            float[,] kernel = {
        { 1, 2, 1 },
        { 2, 40, 2 },
        { 1, 2, 1 }
    };
            ApplyConvolution(kernel, 16f);
            ConfigurarChart();
        }

        private void label13_Click(object sender, EventArgs e)
        {
            float[,] kernel = {
        { -2, -1, 0 },
        { -1, 1, 1 },
        {  0, 1, 2 }
    };
            ApplyConvolution(kernel, 1f);
            ConfigurarChart();
        }

        private void label14_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            Bitmap bmp = new Bitmap(pictureBox4.Image);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            IntPtr ptr = data.Scan0;
            int bytes = Math.Abs(stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            for (int i = 0; i < bytes; i++)
            {
                int ruido = rnd.Next(-30, 30);
                int val = rgbValues[i] + ruido;
                rgbValues[i] = (byte)Clamp(val, 0, 255);
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            bmp.UnlockBits(data);
            pictureBox5.Image = bmp;
            ConfigurarChart();
        }

        private void label15_Click(object sender, EventArgs e)
        {
            int brillo = 20;
            Bitmap bmp = new Bitmap(pictureBox4.Image);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            IntPtr ptr = data.Scan0;
            int bytes = Math.Abs(stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            for (int i = 0; i < bytes; i++)
            {
                int val = rgbValues[i] + brillo;
                rgbValues[i] = (byte)Clamp(val, 0, 255);
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            bmp.UnlockBits(data);
            pictureBox5.Image = bmp;
            ConfigurarChart();
        }

        private void ApplyConvolution(float[,] kernel, float factor)
        {
            Bitmap src = new Bitmap(pictureBox4.Image);
            Bitmap dst = new Bitmap(src.Width, src.Height);
            int w = src.Width;
            int h = src.Height;

            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    float r = 0, g = 0, b = 0;
                    for (int fy = -1; fy <= 1; fy++)
                    {
                        for (int fx = -1; fx <= 1; fx++)
                        {
                            Color pixel = src.GetPixel(x + fx, y + fy);
                            float k = kernel[fy + 1, fx + 1];
                            r += pixel.R * k;
                            g += pixel.G * k;
                            b += pixel.B * k;
                        }
                    }
                    r = (int)Clamp(r / factor, 0, 255);
                    g = (int)Clamp(g / factor, 0, 255);
                    b = (int)Clamp(b / factor, 0, 255);
                    dst.SetPixel(x, y, Color.FromArgb((int)r, (int)g, (int)b));
                }
            }
            pictureBox5.Image = dst;
        }

        private float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }


        public void CalcularHistograma(Bitmap bmp, out int[] histR, out int[] histG, out int[] histB)
        {
            histR = new int[256];
            histG = new int[256];
            histB = new int[256];

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                           ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                int stride = data.Stride;
                int width = bmp.Width;
                int height = bmp.Height;

                for (int y = 0; y < height; y++)
                {
                    byte* row = ptr + (y * stride);
                    for (int x = 0; x < width; x++)
                    {
                        byte b = row[x * 3];
                        byte g = row[x * 3 + 1];
                        byte r = row[x * 3 + 2];

                        histR[r]++;
                        histG[g]++;
                        histB[b]++;
                    }
                }
            }

            bmp.UnlockBits(data);
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }


        private void ConfigurarChart()
        {
            chart1.ChartAreas.Clear();
            chart1.Series.Clear();

            ChartArea area = new ChartArea();
            chart1.ChartAreas.Add(area);

            for (int i = 0; i < 3; i++)
            {
                Series serie = new Series
                {
                    ChartType = SeriesChartType.Column,
                    IsVisibleInLegend = false
                };

                switch (i)
                {
                    case 0:
                        serie.Color = Color.Red;
                        serie.Name = "Rojo";
                        break;
                    case 1:
                        serie.Color = Color.Green;
                        serie.Name = "Verde";
                        break;
                    case 2:
                        serie.Color = Color.Blue;
                        serie.Name = "Azul";
                        break;
                }

                chart1.Series.Add(serie);
            }

            if (pictureBox5.Image != null)
            {
                Bitmap imgActual = new Bitmap(pictureBox5.Image);
                DibujarHistograma(imgActual);
            }
        }

        private void DibujarHistograma(Bitmap imagen)
        {
            int[] histogramaRojo = new int[256];
            int[] histogramaVerde = new int[256];
            int[] histogramaAzul = new int[256];

            for (int y = 0; y < imagen.Height; y++)
            {
                for (int x = 0; x < imagen.Width; x++)
                {
                    Color color = imagen.GetPixel(x, y);
                    histogramaRojo[color.R]++;
                    histogramaVerde[color.G]++;
                    histogramaAzul[color.B]++;
                }
            }

            chart1.Series["Rojo"].Points.Clear();
            chart1.Series["Verde"].Points.Clear();
            chart1.Series["Azul"].Points.Clear();

            for (int i = 0; i < 256; i++)
            {
                chart1.Series["Rojo"].Points.AddXY(i, histogramaRojo[i]);
                chart1.Series["Verde"].Points.AddXY(i, histogramaVerde[i]);
                chart1.Series["Azul"].Points.AddXY(i, histogramaAzul[i]);
            }
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            MostrarHistograma("Rojo");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MostrarHistograma("Azul");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            MostrarHistograma("Verde");
        }

        private void MostrarHistograma(string color)
        {
            foreach (var serie in chart1.Series)
            {
                serie.IsVisibleInLegend = false;
                serie.Enabled = false;
            }

            switch (color)
            {
                case "Rojo":
                    chart1.Series["Rojo"].IsVisibleInLegend = true;
                    chart1.Series["Rojo"].Enabled = true;
                    break;
                case "Verde":
                    chart1.Series["Verde"].IsVisibleInLegend = true;
                    chart1.Series["Verde"].Enabled = true;
                    break;
                case "Azul":
                    chart1.Series["Azul"].IsVisibleInLegend = true;
                    chart1.Series["Azul"].Enabled = true;
                    break;
            }
        }


    }
}
