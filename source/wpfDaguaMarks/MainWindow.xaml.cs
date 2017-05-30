using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing.Imaging;
using System.Data;
using Microsoft.Win32;
using System.IO;
using winForms = System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace wpfDaguaMarks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // STRINGS E OBJETOS UTILIZADOS
        string OriArquivo, MarkFile, DestinoArquivo;
        System.Drawing.Image imgOri, imgMark;
        ImageFormat Formato;
        int Total;
        DataTable dtArquivos;
        int HeightAjus = 0, WidthAjus = 0;
        VMListaImagens lstImgs = new VMListaImagens();

        //List<ImageItem> lstImgItems = new List<ImageItem>();
        List<String> listImgs = new List<string>();

        // OPEN DIALOGS
        OpenFileDialog adcImagemDialog = new OpenFileDialog();

        int ETAPA = 0;

        

        public class VMListaImagens
        {
            public VMListaImagens()
            {
                Items = new List<ImageItem>();
            }
            public List<ImageItem> Items { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();            
            listView.DataContext = null;
            Formato = ImageFormat.Png;
            using (dtArquivos = new DataTable("files"))
            {
                dtArquivos.Columns.Add("ori", typeof(string));
                dtArquivos.Columns.Add("nome", typeof(string));
            }
            cbTransparencia.IsEnabled = true;
            cbTransparencia.IsChecked = true;
            rbTranspHEX.IsEnabled = true;
            rbTranspHEX.IsChecked = true;
            tbTranspHEX.Text = "#FFFFFF";
            ETAPA = 0;
            tab1.IsEnabled = false;
            tab2.IsEnabled = false;
            tab3.IsEnabled = false;
            btnVoltarEtapa.IsEnabled = false;

            cbTamanhoAcao.Items.Add("REDUZIR");
            cbTamanhoAcao.Items.Add("AUMENTAR");
            cbTamanhoAcao.SelectedIndex = 0;
            btnVoltarEtapa.Visibility = Visibility.Hidden;
        }

        private void addImagemClick()
        {
            // Propriedades
            adcImagemDialog.Multiselect = true;
            adcImagemDialog.Title = "Selecionar Imagens...";
            adcImagemDialog.CheckFileExists = true;
            adcImagemDialog.CheckPathExists = true;
            adcImagemDialog.RestoreDirectory = true;

            // Tipos de Imagem
            adcImagemDialog.Filter = "Imagens (*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" + "Todos Arquivos (*.*)|*.*";
            
            // Após Selecionado
            try
            {
                if (adcImagemDialog.ShowDialog() == true)
                {
                    Stream stream;

                    foreach (string file in adcImagemDialog.FileNames)
                    {
                        try
                        {
                            if ((stream = adcImagemDialog.OpenFile()) != null)
                            {
                                using (stream)
                                {
                                    // Novo
                                    ImageItem item = new ImageItem();
                                    item.Nome = System.IO.Path.GetFileName(file).ToLower().Replace(".jpg", null).Replace(".jpeg", null).Replace(".png", null).Replace(".tiff", null).Replace(".bmp", null);
                                    item.Caminho = file;
                                    lstImgs.Items.Add(item);


                                    dtArquivos.Rows.Add(file, System.IO.Path.GetFileName(file).ToLower().Replace(".jpg", null).Replace(".jpeg", null).Replace(".png", null).Replace(".tiff", null).Replace(".bmp", null));
                                    //listImgs.Items.Add(System.IO.Path.GetFileName(file)); < ANTIGO
                                    listImgs.Add(System.IO.Path.GetFileName(file));
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    listView.DataContext = null;
                    listView.DataContext = lstImgs;

                    // Listbox Add
                    //listImgs.Items.Add(adcImagemDialog.FileName);
                    /*listImgs.Items.Add(adcImagemDialog.SafeFileName);
                    using (dtArquivos)
                    {
                        dtArquivos.Rows.Add(adcImagemDialog.FileName, adcImagemDialog.SafeFileName.ToLower().Replace(".jpg", null).Replace(".jpeg", null).Replace(".png", null).Replace(".tiff", null).Replace(".bmp", null));
                    }*/
                }
            }
            catch (Exception ex)
            {
                // ERRO
                MessageBox.Show("Erro: " + ex);
                this.Close();
            }
        }

        private void btnSelecionarLogo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofdLogo = new OpenFileDialog();

            // Propriedades
            ofdLogo.Multiselect = false;
            ofdLogo.Title = "Selecionar o LOGO...";
            ofdLogo.CheckFileExists = true;
            ofdLogo.CheckPathExists = true;
            ofdLogo.RestoreDirectory = true;

            // Tipos de Imagem
            ofdLogo.Filter = "Imagens (*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" + "Todos Arquivos (*.*)|*.*";

            if(ofdLogo.ShowDialog() == true)
            {
                if(ofdLogo.FileName.ToString().Trim() != "")
                {
                    mediaLogo.Source = new Uri(ofdLogo.FileName.ToString());
                    tbCaminhoMark.Text = ofdLogo.FileName.ToString();
                }
            }
        }

        private void btnSelecionarDestino_Click(object sender, RoutedEventArgs e)
        {
            winForms.FolderBrowserDialog setDestino = new winForms.FolderBrowserDialog();
            setDestino.ShowNewFolderButton = true;
            winForms.DialogResult selDestino = setDestino.ShowDialog();
            if (selDestino == winForms.DialogResult.OK)
            {
                tbDestino.Text = setDestino.SelectedPath;
            }
        }

        private void btnConcluirInserirMarcas()
        {
            if (Verificar(tbCaminhoMark) == true)
            {
                Total = lstImgs.Items.Count;
                foreach (ImageItem imgItem in listView.Items)
                {

                    //MessageBox.Show(item.ToLower().Replace(".jpg", null).Replace(".jpeg", null).Replace(".png", null).Replace(".tiff", null).Replace(".bmp", null));
                    // Declarações para Extrair Imagens
                    OriArquivo = imgItem.Caminho;
                    //NomeDoArquivo = Nomes[].ToString();
                    // OriArquivo = item;
                    MarkFile = tbCaminhoMark.Text;
                    DestinoArquivo = tbDestino.Text + "\\" + imgItem.Nome;

                    //FAZER VERIFICAÇÃO DE TAMANHOS

                    try
                    {
                        // Inicia - Faz String > IMAGE <- Original
                        using (imgOri = System.Drawing.Image.FromFile(OriArquivo, true))
                        {
                            // Faz String > IMAGE <- Marca
                            using (imgMark = System.Drawing.Image.FromFile(MarkFile, true))
                            {
                                using (Graphics gpcs = Graphics.FromImage(imgOri))
                                {
                                    // IDENTIFICA EXTENÇÃO
                                    if (rbJPG.IsChecked == true)
                                        Formato = ImageFormat.Jpeg;
                                    if (rbBMP.IsChecked == true)
                                        Formato = ImageFormat.Bmp;
                                    if (rbPNG.IsChecked == true)
                                        Formato = ImageFormat.Png;

                                    // Cria Transparencia se Necessário / Apartir de Agora só se usa "imgMarkFinal"
                                    // Ao invés de usar "imgMark"
                                    Bitmap imgMarkFinal = new Bitmap(imgMark);
                                    // HEXADECIMAL TRANSPARENCIA
                                    /*if (rbTranspHEX.Checked)
                                        imgMarkFinal.MakeTransparent(ColorTranslator.FromHtml(tbTranspHEX.Text));*/

                                    // Inicia Manipulação de Imagens
                                    // Qualidades
                                    gpcs.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                    gpcs.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                                    // BORDAS
                                    int margemBordas = 0;
                                    int.TryParse(txtMargemBordas.Text, out margemBordas);

                                    // Tamanho do Arquivo e Posicionamento
                                    int intWidth = imgMarkFinal.Width;
                                    int intHeight = imgMarkFinal.Size.Height;
                                    if (HeightAjus != 0 && WidthAjus != 0)
                                    {
                                        intWidth = WidthAjus;
                                        intHeight = HeightAjus;
                                    }

                                    // Padrão = Rb3 (Canto Inferior Direito)
                                    int intLeft = imgOri.Width - intWidth;
                                    int intTop = imgOri.Height - intHeight;

                                    // Verifica Posições
                                    if (rb1.IsChecked ==  true)
                                    {
                                        intLeft = (imgOri.Width - imgOri.Width) + margemBordas;
                                        //intTop = imgOri.Height - intHeight;
                                        intTop = (imgOri.Height - intHeight) - margemBordas;
                                    }

                                    else if (rb2.IsChecked == true)
                                    {
                                        intLeft = (imgOri.Width / 2) - (intWidth / 2);
                                        intTop = (imgOri.Height - intHeight) - margemBordas;
                                    }
                                    else if (rb3.IsChecked == true)
                                    {
                                        intLeft = (imgOri.Width - intWidth) - margemBordas;
                                        intTop = (imgOri.Height - intHeight) - margemBordas;
                                    }
                                    else if (rb4.IsChecked == true)
                                    {
                                        intLeft = imgOri.Width - imgOri.Width + margemBordas;
                                        intTop = (imgOri.Height / 2) - (intHeight / 2);
                                    }
                                    else if (rb6.IsChecked == true)
                                    {
                                        intLeft = imgOri.Width - intWidth - margemBordas;
                                        intTop = (imgOri.Height / 2) - (intHeight / 2);
                                    }
                                    else if (rb7.IsChecked == true)
                                    {
                                        intLeft = imgOri.Width - imgOri.Width + margemBordas;
                                        intTop = imgOri.Height - imgOri.Height + margemBordas;
                                        
                                    }
                                    else if (rb8.IsChecked == true)
                                    {
                                        intLeft = (imgOri.Width / 2) - (intWidth / 2);
                                        intTop = (imgOri.Height - imgOri.Height) + margemBordas;
                                    }
                                    else if (rb9.IsChecked == true)
                                    {
                                        intLeft = imgOri.Width - intWidth - margemBordas;
                                        intTop = (imgOri.Height - imgOri.Height) + margemBordas;
                                    }
                                    // Faz Imagem
                                    //gpcs.DrawImage(imgMarkFinal, intLeft - margemBordas, intTop - margemBordas, intWidth, intHeight);
                                    gpcs.DrawImage(imgMarkFinal, intLeft, intTop, intWidth, intHeight);

                                }

                            }
                            // Salvar em Todos Formatos
                            if (rbTodos.IsChecked == true)
                            {
                                Formato = ImageFormat.Jpeg;
                                imgOri.Save(DestinoArquivo + "." + Formato.ToString().ToLower(), Formato);
                                Formato = ImageFormat.Bmp;
                                imgOri.Save(DestinoArquivo + "." + Formato.ToString().ToLower(), Formato);
                                Formato = ImageFormat.Png;
                                imgOri.Save(DestinoArquivo + "." + Formato.ToString().ToLower(), Formato);
                            }
                            else
                                imgOri.Save(DestinoArquivo + "." + Formato.ToString().ToLower(), Formato);
                        }
                    }
                    catch (Exception ex)
                    {
                        // ERRO
                        MessageBox.Show("Erro: " + ex);
                        this.Close();
                    }
                }
                if (Total == lstImgs.Items.Count)
                {
                    // Abre Pasta
                    Process.Start(tbDestino.Text);
                }
            }
        }


        public bool Verificar(TextBox CaminhoMARCA)
        {
            try
            {
                // Verificar Tamanho da Imagem Original
                foreach (ImageItem imgItem in listView.Items)
                {
                    using (System.Drawing.Image Originalxd = System.Drawing.Image.FromFile(imgItem.Caminho.ToString(), true))
                    {
                        using (System.Drawing.Image Marcaxd = System.Drawing.Image.FromFile(CaminhoMARCA.Text, true))
                        {
                            if (Marcaxd.Height > Originalxd.Height || Marcaxd.Width > Originalxd.Width)
                            {
                                MessageBox.Show("A Imagem de Marca é Maior que a Imagem a ser aplicada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                                return false;
                            }
                        }
                    }
                }

                if (lstImgs.Items.Count < 1)
                {
                    MessageBox.Show("Nenhuma Imagem Adicionada!.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (tbCaminhoMark.Text.Length < 1 || tbCaminhoMark.Text == null)
                {
                    MessageBox.Show("A Marca D'agua não foi Selecionada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (rb1.IsChecked == false && rb2.IsChecked == false &&
                    rb3.IsChecked == false && rb4.IsChecked == false &&
                    rb6.IsChecked == false && rb7.IsChecked == false &&
                    rb8.IsChecked == false && rb9.IsChecked == false)
                {
                    MessageBox.Show("Posição da Marca D'agua não Selecionada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (cbTransparencia.IsChecked == true)
                {
                    if (rbTranspHEX.IsChecked == true)
                    {
                        if (tbTranspHEX.IsEnabled == true && tbTranspHEX.Text.Length < 2)
                        {
                            MessageBox.Show("Digite a chave de Alpha em Hexadecimal com ao menos 6 Digitos!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Alpha de Máscara Ativada, Selecione o Tipo!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    if (rbTranspRGB.IsChecked == true)
                    {
                        if (tbTranspR.Text.Length < 3 || tbTranspG.Text.Length < 3 || tbTranspB.Text.Length < 3)
                        {
                            MessageBox.Show("A Transparencia de Máscara está ativada, por favor corrija os valores de RGB!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                    }
                }

                if (tbDestino.Text.Length < 1 || tbDestino.Text == null)
                {
                    MessageBox.Show("Caminho Destino não Encontrado!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (rbBMP.IsChecked == false && rbJPG.IsChecked == false && rbPNG.IsChecked == false && rbTodos.IsChecked == false)
                {
                    MessageBox.Show("Selecione um Formato para Exportação!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                MessageBox.Show("Sucesso", "Sucesso");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERRO na Verificação.");
                return false;
            }
        }


        private void btnConcluir_Click(object sender, RoutedEventArgs e)
        {
            int ok = 0;
            int.TryParse(slider.Value.ToString(), out ok);
            AjustarMarca(cbTamanhoAcao, ok);
            btnConcluirInserirMarcas();
        }

        private void cbTransparencia_Checked(object sender, RoutedEventArgs e)
        {
            rbTranspHEX.IsEnabled = true;
            rbTranspRGB.IsEnabled = true;
        }

        private void rbTranspHEX_Checked(object sender, RoutedEventArgs e)
        {
            tbTranspR.IsEnabled = false;
            tbTranspG.IsEnabled = false;
            tbTranspB.IsEnabled = false;
            tbTranspHEX.IsEnabled = true;
        }

        private void rbTranspRGB_Checked(object sender, RoutedEventArgs e)
        {
            tbTranspR.IsEnabled = true;
            tbTranspG.IsEnabled = true;
            tbTranspB.IsEnabled = true;
            tbTranspHEX.IsEnabled = false;
        }

        private void cbTransparencia_Unchecked(object sender, RoutedEventArgs e)
        {
            rbTranspHEX.IsEnabled = false;
            rbTranspRGB.IsEnabled = false;
            tbTranspR.IsEnabled = false;
            tbTranspG.IsEnabled = false;
            tbTranspB.IsEnabled = false;
            tbTranspHEX.IsEnabled = false;
        }


        private void checkEtapas()
        {
            switch (ETAPA)
            {
                case 0:
                    if (listView.Items.Count > 0)
                    {
                        ETAPA = 1;
                        tab2.IsSelected = true;
                        btnVoltarEtapa.IsEnabled = true;
                        btnVoltarEtapa.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MessageBox.Show("Necessário Adicionar imagens.", "Erro", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    break;

                case 1:
                    if (tbCaminhoMark.Text.Trim() != "")
                    {
                        ETAPA = 2;
                        tab3.IsSelected = true;
                        btnVoltarEtapa.IsEnabled = true;
                        btnProximaEtapa.IsEnabled = false;
                        btnVoltarEtapa.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MessageBox.Show("Necessário Selecionar o MARCA D'AGUA.", "Erro", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    break;

                case 2:
                    btnVoltarEtapa.Visibility = Visibility.Visible;
                    break;

            }
        }

        private void btnProximaEtapa_Click(object sender, RoutedEventArgs e)
        {
            checkEtapas();
        }

        private void btnVoltarEtapa_Click(object sender, RoutedEventArgs e)
        {
            if(ETAPA > 0)
            {
                ETAPA--;
            }

            switch(ETAPA)
            {
                case 0:
                    tab1.IsSelected = true;
                    btnVoltarEtapa.IsEnabled = false;
                    btnVoltarEtapa.Visibility = Visibility.Hidden;
                    break;

                case 1:
                    tab2.IsSelected = true;
                    btnVoltarEtapa.Visibility = Visibility.Visible;
                    btnVoltarEtapa.IsEnabled = true;
                    btnProximaEtapa.IsEnabled = true;
                    
                    break;

                case 2:
                    btnVoltarEtapa.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void slider_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tbPorcentagem.Text = slider.Value.ToString();
            int ok = 0;
            int.TryParse(slider.Value.ToString(), out ok);
            AjustarMarca(cbTamanhoAcao, ok);
        }


        public void AjustarMarca(ComboBox op, int porcent)
        {
            if (tbCaminhoMark.Text != null)
            {
                if (op.Text.ToLower() == "aumentar")
                {

                }
                else if (op.Text.ToLower() == "reduzir")
                {
                    // Verificar Tamanho da Imagem Original
                    using (System.Drawing.Image Marcaxd = System.Drawing.Image.FromFile(tbCaminhoMark.Text, true))
                    {
                        int valorint = Convert.ToInt32(porcent);
                        WidthAjus = Marcaxd.Width - ((Marcaxd.Width / 100) * valorint);
                        HeightAjus = Marcaxd.Height - ((Marcaxd.Height / 100) * valorint);
                        lblLargura.Content = WidthAjus+" px";
                        lblAltura.Content = HeightAjus + " px";
                    }

                }
            }
            else
                MessageBox.Show("Erro, Marca nao Selecionada.");
        }


        private void remImagensClick()
        {
            if (lstImgs.Items.Count > 0)
            {
                try
                {
                    foreach (DataRow dtFiles in dtArquivos.Rows)
                    {
                        string ori = dtFiles[1].ToString();
                        ImageItem ok = lstImgs.Items.Find(x => x.Caminho == ori);
                        if (ok != null)
                        {
                            lstImgs.Items.Remove(ok);
                        }
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Erro ao Deletar: " + ex.Message, "Erro");
                }
            }
        }

        private void btnAddImg_Click(object sender, RoutedEventArgs e)
        {
            addImagemClick();
        }

        private void btnRemImgs_Click(object sender, RoutedEventArgs e)
        {
            foreach (ImageItem img in listView.SelectedItems)
            {
                lstImgs.Items.Remove(img);
            }
            listView.DataContext = null;
            listView.DataContext = lstImgs;
        }

        private void btnLimparTudo_Click(object sender, RoutedEventArgs e)
        {
            foreach (ImageItem img in listView.Items)
            {
                lstImgs.Items.Remove(img);
            }
            listView.DataContext = null;
            listView.DataContext = lstImgs;
        }
    }


    public class ImageItem
    {
        public string Nome { get; set; }
        public string Caminho { get; set; }
    }



}
