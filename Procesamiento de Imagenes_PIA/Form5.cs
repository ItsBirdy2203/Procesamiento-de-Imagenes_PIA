using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Imaging;

using Accord.Video.DirectShow;


namespace Procesamiento_de_Imagenes_PIA
{
    public partial class Form5 : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private Bitmap currentFrame;

        private bool isHovered = false;
        private bool isClicked = false;
        private Button activeButton = null;






        private bool filtroActivo = false;
        private FilterInfoCollection dispositivosVideo;
        private VideoCaptureDevice fuenteVideo;

        private string colorSeleccionado = "Red"; // Para gráficas o lógica previa
        private Color colorDetectado = Color.Red; // Color para detección real en imagen

        private readonly object _lock = new object();
        private volatile bool _procesandoFrame = false;

        private Bitmap imagenActual = null;







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
        public Form5()
        {
           
            InitializeComponent();
            AddAppleButtons();
            this.FormClosing += Form4_FormClosing;
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

        private void Form5_Load(object sender, EventArgs e)
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


   
   

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }


        















        //lo mero
        private void panel2_Click(object sender, EventArgs e)
        {
            dispositivosVideo = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (dispositivosVideo.Count > 0)
            {
                fuenteVideo = new VideoCaptureDevice(dispositivosVideo[0].MonikerString);

                var resoluciones = fuenteVideo.VideoCapabilities;
                if (resoluciones.Length > 0)
                {
                    fuenteVideo.VideoResolution = resoluciones[0];
                }

                fuenteVideo.NewFrame += FuenteVideo_NewFrame;
                fuenteVideo.Start();
            }
            else
            {
                MessageBox.Show("No se encontró ninguna cámara.");
            }
        }

        private void FuenteVideo_NewFrame(object sender, Accord.Video.NewFrameEventArgs eventArgs)
        {
            if (_procesandoFrame) return;
            _procesandoFrame = true;

            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();

            try
            {
                Bitmap imagenAMostrar;

                if (filtroActivo)
                {
                    imagenAMostrar = AplicarFiltroColor(frame, colorDetectado, 180);

                }
                else
                {
                    imagenAMostrar = (Bitmap)frame.Clone();
                }

                ImageStatistics stats = new ImageStatistics(frame);

                this.Invoke(new Action(() =>
                {
                    lock (_lock)
                    {
                        if (imagenActual != null)
                        {
                            imagenActual.Dispose();
                            imagenActual = null;
                        }

                        imagenActual = imagenAMostrar;

                        pictureBox2.Image = imagenActual;

                        ActualizarGrafica(stats);
                    }
                }));
            }
            finally
            {
                frame.Dispose();
                _procesandoFrame = false;
            }
        }

        private Bitmap AplicarFiltroColor(Bitmap src, Color colorObjetivo, int umbral)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            var rect = new Rectangle(0, 0, src.Width, src.Height);

            var srcData = src.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var dstData = dst.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;

                int stride = srcData.Stride;

                for (int y = 0; y < src.Height; y++)
                {
                    byte* srcRow = srcPtr + (y * stride);
                    byte* dstRow = dstPtr + (y * stride);

                    for (int x = 0; x < src.Width; x++)
                    {
                        byte b = srcRow[x * 3];
                        byte g = srcRow[x * 3 + 1];
                        byte r = srcRow[x * 3 + 2];

                        int dr = r - colorObjetivo.R;
                        int dg = g - colorObjetivo.G;
                        int db = b - colorObjetivo.B;

                        double distancia = Math.Sqrt(dr * dr + dg * dg + db * db);

                        if (distancia <= umbral)
                        {
                            dstRow[x * 3] = b;
                            dstRow[x * 3 + 1] = g;
                            dstRow[x * 3 + 2] = r;
                        }
                        else
                        {
                            byte gris = (byte)(r * 0.3 + g * 0.59 + b * 0.11);
                            dstRow[x * 3] = gris;
                            dstRow[x * 3 + 1] = gris;
                            dstRow[x * 3 + 2] = gris;
                        }
                    }
                }
            }

            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);

            return dst;
        }


        private void ActualizarGrafica(ImageStatistics stats)
        {
            for (int i = 0; i < 256; i++)
            {
                chart1.Series["Red"].Points[i].YValues[0] = stats.Red.Values[i];
                chart1.Series["Green"].Points[i].YValues[0] = stats.Green.Values[i];
                chart1.Series["Blue"].Points[i].YValues[0] = stats.Blue.Values[i];
            }

            foreach (var serie in chart1.Series)
            {
                serie.Enabled = serie.Name.Equals(colorSeleccionado, StringComparison.OrdinalIgnoreCase);
                serie.IsVisibleInLegend = serie.Enabled;
            }

            int maxY = 1;
            switch (colorSeleccionado.ToLower())
            {
                case "red":
                    maxY = stats.Red.Max;
                    break;
                case "green":
                    maxY = stats.Green.Max;
                    break;
                case "blue":
                    maxY = stats.Blue.Max;
                    break;
            }
            if (maxY == 0) maxY = 1;

            chart1.ChartAreas[0].AxisY.Maximum = maxY;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.Invalidate();
        }

        private void ConfigurarChart()
        {
            chart1.ChartAreas.Clear();
            chart1.Series.Clear();

            chart1.ChartAreas.Add(new System.Windows.Forms.DataVisualization.Charting.ChartArea());

            string[] colores = { "Red", "Green", "Blue" };
            Color[] coloresRGB = { Color.Red, Color.Green, Color.Blue };

            for (int i = 0; i < 3; i++)
            {
                var serie = new System.Windows.Forms.DataVisualization.Charting.Series(colores[i])
                {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                    Color = coloresRGB[i],
                    IsVisibleInLegend = false
                };

                for (int j = 0; j < 256; j++)
                    serie.Points.AddXY(j, 0);

                chart1.Series.Add(serie);
            }
        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (fuenteVideo != null && fuenteVideo.IsRunning)
            {
                fuenteVideo.SignalToStop();
                fuenteVideo.WaitForStop();
            }

            lock (_lock)
            {
                if (imagenActual != null)
                {
                    imagenActual.Dispose();
                    imagenActual = null;
                }
            }
        }

        private void label6_Click(object sender, EventArgs e) => colorSeleccionado = "Red";
        private void label7_Click(object sender, EventArgs e) => colorSeleccionado = "Blue";
        private void label8_Click(object sender, EventArgs e) => colorSeleccionado = "Green";

       

    


        private void panel3_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlg = new ColorDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    colorDetectado = dlg.Color;
                    filtroActivo = true;

                }
            }
        }

        

    }
}
