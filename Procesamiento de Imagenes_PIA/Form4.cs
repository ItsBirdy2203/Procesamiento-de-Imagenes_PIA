using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace Procesamiento_de_Imagenes_PIA
{
    public partial class Form4 : Form
    {

        private VideoFileReader reader;
        private CancellationTokenSource cts;
        private bool isPlaying = false;
        private Thread playThread;
        private bool isPaused = false;
        private string videoPath = "";
        int brillo = 50;

       


        private enum FiltroTipo { Ninguno, Brillo, Posterizacion, Negativo, Gaussiano, Solarizado, Contorno, Intensidad, Matiz, Emboss, Ruido}
        private FiltroTipo filtroActual = FiltroTipo.Ninguno;




        private bool isHovered = false;
        private bool isClicked = false;
        private Button activeButton = null;
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
        public Form4()
        {
            InitializeComponent();
            
            AddAppleButtons();
          



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



        private void Form4_Load(object sender, EventArgs e)
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

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {
            filtroActual = FiltroTipo.Brillo;
        }




        private void label8_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Videos|*.mp4;*.avi;*.mov;*.mkv"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                videoPath = ofd.FileName;
                StartVideo(); 
            }
        }

        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {

        }


        private async void pictureBox5_Click(object sender, EventArgs e)
        {
            if (!isPlaying && !string.IsNullOrEmpty(videoPath))
            {
                StartVideo(); // Si estaba detenido, vuelve a empezar
            }
            else
            {
                isPaused = false; // Reanuda
            }
        }

        private void StartVideo()
        {
            StopVideo(); // Detener si ya hay uno reproduciendo

            reader = new VideoFileReader();
            reader.Open(videoPath);

            cts = new CancellationTokenSource();
            playThread = new Thread(() =>
            {
                double fps = (double)reader.FrameRate.Numerator / reader.FrameRate.Denominator;
                int delay = (int)(1000 / fps);

                isPlaying = true;
                isPaused = false;

                while (reader.IsOpen && !cts.Token.IsCancellationRequested)
                {
                    if (isPaused)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    Bitmap frame = reader.ReadVideoFrame();
                    if (frame == null) break;

                    Bitmap filteredFrame = AplicarFiltro(frame);

                    pictureBox4.Invoke((Action)(() =>
                    {
                        pictureBox4.Image?.Dispose();
                        pictureBox4.Image = (Bitmap)filteredFrame.Clone();
                    }));

                    frame.Dispose();
                    filteredFrame.Dispose();


                    Thread.Sleep(delay);
                    frame.Dispose();
                }

                isPlaying = false;
                reader.Close();
            });

            playThread.IsBackground = true;
            playThread.Start();
        }


        private void StopVideo()
        {
            if (isPlaying)
            {
                cts?.Cancel();
                playThread?.Join();
                isPlaying = false;
            }

            if (reader != null && reader.IsOpen)
            {
                reader.Close();
            }

            pictureBox4.Image?.Dispose();
            pictureBox4.Image = null;
        }




        private void pictureBox6_Click(object sender, EventArgs e)
        {
            isPaused = true;
        }

      
        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopVideo(); // Detiene y limpia
        }

        private void label5_Click(object sender, EventArgs e)
        {
            filtroActual = FiltroTipo.Posterizacion;
        }

        private void label6_Click(object sender, EventArgs e)
        {
            filtroActual = FiltroTipo.Negativo;
        }

        private void label7_Click(object sender, EventArgs e)
        {
         filtroActual = FiltroTipo.Solarizado;
        }


        private Bitmap AplicarFiltro(Bitmap original)
        {
            Bitmap bmp = new Bitmap(original.Width, original.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(original, 0, 0);
            }

            switch (filtroActual)
            {
                case FiltroTipo.Brillo:
                    return CambiarBrillo(bmp, 40);
                case FiltroTipo.Posterizacion:
                    return AplicarPosterizacion(bmp, 6);
                case FiltroTipo.Negativo:
                    return AplicarNegativo(bmp);
                case FiltroTipo.Solarizado:
                    return AplicarSolarizado(bmp, 100);
                case FiltroTipo.Gaussiano:
                    return AplicarGaussiano(bmp);
                case FiltroTipo.Contorno:
                    return AplicarContorno(bmp);
                case FiltroTipo.Intensidad:
                    return AplicarIntensidad(bmp,2.3f);
                case FiltroTipo.Matiz:
                    return AplicarMatiz(bmp, 120f);
                case FiltroTipo.Emboss:
                    return AplicarEmboss(bmp);
                case FiltroTipo.Ruido:
                    return AplicarRuido(bmp);
                default:
                    return bmp;
            }
        }

        private Bitmap CambiarBrillo(Bitmap img, int value)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);

            BitmapData srcData = img.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;
            IntPtr ptrSrc = srcData.Scan0;
            IntPtr ptrDst = dstData.Scan0;

            int bytes = Math.Abs(stride) * img.Height;
            byte[] buffer = new byte[bytes];

            Marshal.Copy(ptrSrc, buffer, 0, bytes);

            for (int i = 0; i < bytes; i++)
            {
                int val = buffer[i] + brillo;
                buffer[i] = (byte)Math.Max(0, Math.Min(255, val));
            }

            Marshal.Copy(buffer, 0, ptrDst, bytes);

            img.UnlockBits(srcData);
            bmp.UnlockBits(dstData);

            return bmp;
        }

        private Bitmap AplicarPosterizacion(Bitmap img, int niveles = 4)
        {
            Bitmap resultado = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);

            int paso = 256 / niveles;

            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);

            BitmapData srcData = img.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = resultado.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;
            IntPtr ptrSrc = srcData.Scan0;
            IntPtr ptrDst = dstData.Scan0;

            int bytes = Math.Abs(stride) * img.Height;
            byte[] bufferSrc = new byte[bytes];
            byte[] bufferDst = new byte[bytes];

            Marshal.Copy(ptrSrc, bufferSrc, 0, bytes);

            for (int i = 0; i < bytes; i += 3)
            {
                // Canales en orden: B G R
                bufferDst[i] = (byte)((bufferSrc[i] / paso) * paso + paso / 2); // Blue
                bufferDst[i + 1] = (byte)((bufferSrc[i + 1] / paso) * paso + paso / 2); // Green
                bufferDst[i + 2] = (byte)((bufferSrc[i + 2] / paso) * paso + paso / 2); // Red
            }

            Marshal.Copy(bufferDst, 0, ptrDst, bytes);

            img.UnlockBits(srcData);
            resultado.UnlockBits(dstData);

            return resultado;
        }


        private Bitmap AplicarNegativo(Bitmap img)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);

            BitmapData dataSrc = img.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dataDst = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = dataSrc.Stride;
            IntPtr ptrSrc = dataSrc.Scan0;
            IntPtr ptrDst = dataDst.Scan0;

            int bytes = Math.Abs(stride) * img.Height;
            byte[] rgbValues = new byte[bytes];
            byte[] result = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(ptrSrc, rgbValues, 0, bytes);

            for (int i = 0; i < bytes; i += 3)
            {
                result[i] = (byte)(255 - rgbValues[i]);       // Blue
                result[i + 1] = (byte)(255 - rgbValues[i + 1]); // Green
                result[i + 2] = (byte)(255 - rgbValues[i + 2]); // Red
            }

            System.Runtime.InteropServices.Marshal.Copy(result, 0, ptrDst, bytes);

            img.UnlockBits(dataSrc);
            bmp.UnlockBits(dataDst);

            return bmp;
        }


        private void label13_Click(object sender, EventArgs e)
        {
            filtroActual = FiltroTipo.Gaussiano;
        }

        private Bitmap AplicarGaussiano(Bitmap img, int repeticiones = 3)
        {
            Bitmap resultado = (Bitmap)img.Clone();

            for (int i = 0; i < repeticiones; i++)
            {
                resultado = AplicarGaussianoUnaVez(resultado);
            }

            return resultado;
        }

        private Bitmap AplicarGaussianoUnaVez(Bitmap img)
        {
            float[,] kernel = {
        { 1f, 2f, 1f },
        { 2f, 4f, 2f },
        { 1f, 2f, 1f }
    };

            float factor = 1f / 16f;

            Bitmap resultado = new Bitmap(img.Width, img.Height);
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);

            BitmapData srcData = img.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = resultado.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;
            IntPtr ptrSrc = srcData.Scan0;
            IntPtr ptrDst = dstData.Scan0;

            int width = img.Width;
            int height = img.Height;

            byte[] srcBuffer = new byte[stride * height];
            byte[] dstBuffer = new byte[stride * height];

            Marshal.Copy(ptrSrc, srcBuffer, 0, srcBuffer.Length);

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    float[] sum = new float[3]; // B, G, R

                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            int px = (x + kx) * 3;
                            int py = (y + ky) * stride;

                            float k = kernel[ky + 1, kx + 1];

                            sum[0] += srcBuffer[py + px] * k;     // Blue
                            sum[1] += srcBuffer[py + px + 1] * k; // Green
                            sum[2] += srcBuffer[py + px + 2] * k; // Red
                        }
                    }

                    int pos = y * stride + x * 3;

                    dstBuffer[pos] = (byte)Math.Min(255, sum[0] * factor); // B
                    dstBuffer[pos + 1] = (byte)Math.Min(255, sum[1] * factor); // G
                    dstBuffer[pos + 2] = (byte)Math.Min(255, sum[2] * factor); // R
                }
            }

            Marshal.Copy(dstBuffer, 0, ptrDst, dstBuffer.Length);
            img.UnlockBits(srcData);
            resultado.UnlockBits(dstData);

            return resultado;
        }


        private Bitmap AplicarSolarizado(Bitmap img, byte umbral = 128)
        {
            Bitmap resultado = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);

            BitmapData srcData = img.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = resultado.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;
            int bytes = Math.Abs(stride) * img.Height;

            byte[] bufferSrc = new byte[bytes];
            byte[] bufferDst = new byte[bytes];

            Marshal.Copy(srcData.Scan0, bufferSrc, 0, bytes);

            for (int i = 0; i < bytes; i += 3)
            {
                byte b = bufferSrc[i];
                byte g = bufferSrc[i + 1];
                byte r = bufferSrc[i + 2];

                bufferDst[i] = b > umbral ? (byte)(255 - b) : b; // Blue
                bufferDst[i + 1] = g > umbral ? (byte)(255 - g) : g; // Green
                bufferDst[i + 2] = r > umbral ? (byte)(255 - r) : r; // Red
            }

            Marshal.Copy(bufferDst, 0, dstData.Scan0, bytes);

            img.UnlockBits(srcData);
            resultado.UnlockBits(dstData);

            return resultado;
        }

        private Bitmap AplicarUmbralLockBits(Bitmap img, byte umbral = 128)
        {
            Bitmap resultado = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);

            BitmapData srcData = img.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = resultado.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;
            int bytes = Math.Abs(stride) * img.Height;

            byte[] bufferSrc = new byte[bytes];
            byte[] bufferDst = new byte[bytes];

            Marshal.Copy(srcData.Scan0, bufferSrc, 0, bytes);

            for (int i = 0; i < bytes; i += 3)
            {
                byte b = bufferSrc[i];
                byte g = bufferSrc[i + 1];
                byte r = bufferSrc[i + 2];

                // Convertimos a escala de grises para calcular el brillo (luminancia)
                byte gris = (byte)(0.3 * r + 0.59 * g + 0.11 * b);

                byte valor = gris > umbral ? (byte)255 : (byte)0;

                bufferDst[i] = valor; // Blue
                bufferDst[i + 1] = valor; // Green
                bufferDst[i + 2] = valor; // Red
            }

            Marshal.Copy(bufferDst, 0, dstData.Scan0, bytes);

            img.UnlockBits(srcData);
            resultado.UnlockBits(dstData);

            return resultado;
        }

        private void label10_Click_1(object sender, EventArgs e)
        {
            filtroActual = FiltroTipo.Contorno;
        }

        private void label11_Click(object sender, EventArgs e)
        {
            filtroActual = FiltroTipo.Intensidad;
        }

        private void label12_Click(object sender, EventArgs e)
        {
            filtroActual = FiltroTipo.Matiz;
        }

        private Bitmap AplicarContorno(Bitmap bmp, byte umbral = 128)
        {
            float[,] kernel = {
                {-1, -1, -1},
                {-1,  8, -1},
                {-1, -1, -1}
            };
            return AplicarKernel(bmp, kernel, 1);
        }

        private Bitmap AplicarIntensidad(Bitmap img, float factor = 1.2f)
        {
            Bitmap resultado = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);

            BitmapData srcData = img.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = resultado.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;
            int bytes = Math.Abs(stride) * img.Height;

            byte[] bufferSrc = new byte[bytes];
            byte[] bufferDst = new byte[bytes];

            Marshal.Copy(srcData.Scan0, bufferSrc, 0, bytes);

            for (int i = 0; i < bytes; i += 3)
            {
                byte b = bufferSrc[i];
                byte g = bufferSrc[i + 1];
                byte r = bufferSrc[i + 2];

                bufferDst[i] = (byte)Math.Min(255, b * factor); // Blue
                bufferDst[i + 1] = (byte)Math.Min(255, g * factor); // Green
                bufferDst[i + 2] = (byte)Math.Min(255, r * factor); // Red
            }

            Marshal.Copy(bufferDst, 0, dstData.Scan0, bytes);

            img.UnlockBits(srcData);
            resultado.UnlockBits(dstData);

            return resultado;
        }

        private Bitmap AplicarMatiz(Bitmap img, float grados = 90f)
        {
            Bitmap resultado = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);

            BitmapData srcData = img.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = resultado.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;
            int bytes = Math.Abs(stride) * img.Height;

            byte[] bufferSrc = new byte[bytes];
            byte[] bufferDst = new byte[bytes];

            Marshal.Copy(srcData.Scan0, bufferSrc, 0, bytes);

            float hueShift = grados / 360f;

            for (int i = 0; i < bytes; i += 3)
            {
                float b = bufferSrc[i] / 255f;
                float g = bufferSrc[i + 1] / 255f;
                float r = bufferSrc[i + 2] / 255f;

                // Convertir RGB a HSL
                Color color = Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
                float h = color.GetHue() / 360f;
                float s = color.GetSaturation();
                float l = color.GetBrightness();

                // Ajustar el matiz
                h = (h + hueShift) % 1f;

                // Convertir HSL de nuevo a RGB
                Color nuevoColor = FromHSL(h, s, l);

                bufferDst[i] = nuevoColor.B;
                bufferDst[i + 1] = nuevoColor.G;
                bufferDst[i + 2] = nuevoColor.R;
            }

            Marshal.Copy(bufferDst, 0, dstData.Scan0, bytes);

            img.UnlockBits(srcData);
            resultado.UnlockBits(dstData);

            return resultado;
        }

        private Color FromHSL(float h, float s, float l)
        {
            if (s == 0)
            {
                byte val = (byte)(l * 255);
                return Color.FromArgb(val, val, val);
            }

            float q = l < 0.5f ? l * (1 + s) : l + s - (l * s);
            float p = 2 * l - q;

            float r = HueToRGB(p, q, h + 1f / 3f);
            float g = HueToRGB(p, q, h);
            float b = HueToRGB(p, q, h - 1f / 3f);

            return Color.FromArgb(
                (int)(r * 255),
                (int)(g * 255),
                (int)(b * 255));
        }

        private float HueToRGB(float p, float q, float t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1f / 6f) return p + (q - p) * 6 * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6;
            return p;
        }


        private unsafe Bitmap AplicarEmboss(Bitmap bmp)
        {
            float[,] kernel = {
                { -2, -1, 0 },
                { -1, 1, 1 },
                { 0, 1, 2 }
            };
            return AplicarKernel(bmp, kernel, 1);
        }
        private unsafe Bitmap AplicarKernel(Bitmap bmp, float[,] kernel, float factor)
        {
            int kSize = kernel.GetLength(0);
            int offset = kSize / 2;

            Bitmap result = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            BitmapData srcData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int height = bmp.Height;
            int width = bmp.Width;
            int stride = srcData.Stride;

            byte* srcPtr = (byte*)srcData.Scan0;
            byte* dstPtr = (byte*)dstData.Scan0;

            for (int y = 0; y < height; y++)
            {
                byte* dstRow = dstPtr + y * stride;

                for (int x = 0; x < width; x++)
                {
                    float sumaB = 0, sumaG = 0, sumaR = 0;

                    for (int ky = 0; ky < kSize; ky++)
                    {
                        int yy = y + ky - offset;
                        if (yy < 0) yy = 0;
                        if (yy >= height) yy = height - 1;

                        byte* srcRow = srcPtr + yy * stride;

                        for (int kx = 0; kx < kSize; kx++)
                        {
                            int xx = x + kx - offset;
                            if (xx < 0) xx = 0;
                            if (xx >= width) xx = width - 1;

                            float k = kernel[ky, kx];
                            sumaB += srcRow[xx * 3] * k;
                            sumaG += srcRow[xx * 3 + 1] * k;
                            sumaR += srcRow[xx * 3 + 2] * k;
                        }
                    }

                    dstRow[x * 3] = (byte)ClampToByte((int)(sumaB / factor));
                    dstRow[x * 3 + 1] = (byte)ClampToByte((int)(sumaG / factor));
                    dstRow[x * 3 + 2] = (byte)ClampToByte((int)(sumaR / factor));
                }
            }

            bmp.UnlockBits(srcData);
            result.UnlockBits(dstData);

            return result;
        }
        private int ClampToByte(int valor)
        {
            if (valor < 0) return 0;
            if (valor > 255) return 255;
            return valor;
        }

        private void label14_Click(object sender, EventArgs e)
        {
            filtroActual = FiltroTipo.Emboss;

        }

        private unsafe Bitmap AplicarRuido(Bitmap bmp)
        {
            Bitmap result = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            BitmapData srcData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int height = bmp.Height;
            int width = bmp.Width;
            int stride = srcData.Stride;

            Random rand = new Random();

            byte* srcPtr = (byte*)srcData.Scan0;
            byte* dstPtr = (byte*)dstData.Scan0;

            for (int y = 0; y < height; y++)
            {
                byte* srcRow = srcPtr + y * stride;
                byte* dstRow = dstPtr + y * stride;

                for (int x = 0; x < width; x++)
                {
                    int ruido = rand.Next(-30, 30);
                    int b = ClampToByte(srcRow[x * 3] + ruido);
                    int g = ClampToByte(srcRow[x * 3 + 1] + ruido);
                    int r = ClampToByte(srcRow[x * 3 + 2] + ruido);

                    dstRow[x * 3] = (byte)b;
                    dstRow[x * 3 + 1] = (byte)g;
                    dstRow[x * 3 + 2] = (byte)r;
                }
            }

            bmp.UnlockBits(srcData);
            result.UnlockBits(dstData);

            return result;
        }

        private void label15_Click(object sender, EventArgs e)
        {
            filtroActual = FiltroTipo.Ruido;
        }
    }
}
