﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using G1Tool.IO;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FETHArchiveManager
{
    public partial class MainWindow : Window
    {
        public DATA0 data0 { get; set; }
        private string data1Filepath;
        public INFO0 info0 { get; set; }
        public INFO2 info2 { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            data0 = (DATA0)this.Resources["data0"];
            info0 = (INFO0)this.Resources["info0"];
            info2 = new INFO2();
        }

        private void AddButtonINFO_Click(object sender, RoutedEventArgs e)
        {
            info0.Add(new INFO0Entry()
            {
                EntryID = 0,
                UncompressedSize = 0x0,
                CompressedSize = 0x0,
                Compressed = false,
                Filepath = "rom:/"
            });
        }

        private void SaveButtonINFO_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog { DefaultExt = ".bin", FileName = "INFO0", Filter = "FETH Patch Binary (INFO0.bin)|*.bin" };

            if (dialog.ShowDialog() == true)
            {
                INFO2 info2 = new INFO2(info0.Count);
                info0.Write(dialog.FileName);
                info2.Write(Path.GetDirectoryName(dialog.FileName) + "/INFO2.bin");
            }
        }

        private void OpenButtonINFO_Click(object sender, RoutedEventArgs e)
        {
            info0.Clear();

            OpenFileDialog dialog = new OpenFileDialog { DefaultExt = ".bin", FileName = "INFO0", Filter = "FETH Patch Binary (INFO0.bin)|*.bin" };

            if (dialog.ShowDialog() == true)
            {
                info0.Read(dialog.FileName);
                if (File.Exists(Path.GetFullPath(dialog.FileName) + "INFO2"))
                    info2.Read(Path.GetFullPath(dialog.FileName) + "INFO2");

            }
        }

        private void RemoveButtonINFO_Click(object sender, RoutedEventArgs e)
        {
            info0.Remove((INFO0Entry)dataGridINFO.SelectedItem);
        }

        private void NewButtonINFO_Click(object sender, RoutedEventArgs e)
        {
            info0.Clear();

            var assembly = Assembly.GetExecutingAssembly();
            string info0ResourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("patch3.INFO0.bin"));
            string info1ResourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("patch3.INFO1.bin"));
            string info2ResourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("patch3.INFO2.bin"));
            info0.Read(new EndianBinaryReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(info0ResourceName), Endianness.Little));
            info2.Read(new EndianBinaryReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(info2ResourceName), Endianness.Little));
        }

        private void DataGridINFO_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!(e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.V))
            {
                return;
            }

            var i = dataGridINFO.SelectedIndex;
            string clipboardData = Clipboard.GetData(DataFormats.Text).ToString();
            foreach (var row in clipboardData.Split(Environment.NewLine.ToCharArray(),StringSplitOptions.RemoveEmptyEntries))
            {
                var cellSet = row.Split('\t');

                long entryId;
                long uncompressedSize;
                long compressedSize;
                bool isCompressed;
                if
                (
                    cellSet.Length != 5 ||
                    !long.TryParse(cellSet[0], out entryId) ||
                    !long.TryParse(cellSet[1].Replace("0x",""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uncompressedSize) ||
                    !long.TryParse(cellSet[2].Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out compressedSize) ||
                    !bool.TryParse(cellSet[3], out isCompressed)
                )
                {
                    continue;
                }

                var newEntry = new INFO0Entry()
                {
                    EntryID = entryId,
                    UncompressedSize = uncompressedSize,
                    CompressedSize = compressedSize,
                    Compressed = isCompressed,
                    Filepath = cellSet[4]
                };

                if (info0.Count > i)
                {
                    info0[i] = newEntry;
                }
                else
                {
                    info0.Add(newEntry);
                }
                i++;
            }
        }

        private void OpenButtonDATA_Click(object sender, RoutedEventArgs e)
        {
            data0.Clear();

            OpenFileDialog dialog = new OpenFileDialog { DefaultExt = ".bin", FileName = "DATA0", Filter = "FETH Data Binary (DATA0.bin)|*.bin" };

            if (dialog.ShowDialog() == true)
            {
                string data1Path = Path.GetDirectoryName(dialog.FileName) + "/DATA1.bin";

                data0.Read(dialog.FileName);

                if (File.Exists(data1Path))
                {
                    data1Filepath = data1Path;
                }
            }
        }

        private void ExtractAllUncompressedButtonDATA_Click(object sender, RoutedEventArgs e)
        {

            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                foreach(DATA0Entry entry in data0)
                {
                    if (entry.CompressedSize == 0 || entry.UncompressedSize == 0)
                        continue;

                    using (EndianBinaryReader r = new EndianBinaryReader(new FileStream( data1Filepath, FileMode.Open ), Endianness.Little))
                    {
                        using (EndianBinaryWriter w = new EndianBinaryWriter(new FileStream(dialog.FileName + "/" + entry.EntryID + ".bin", FileMode.Create), Endianness.Little))
                        {
                            if ( entry.Compressed )
                            {
                                KTGZip zlib = new KTGZip();
                                r.SeekBegin(entry.Offset);
                                w.Write(zlib.Decompress(r.ReadBytes((int)entry.CompressedSize)));
                            }
                            else
                            {
                                r.SeekBegin(entry.Offset);
                                w.Write(r.ReadBytes( ( int ) entry.UncompressedSize));
                            }
                            
                        }
                    }
                }
                
            }
        }

        private void AddButtonDATA_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RemoveButtonDATA_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Extract_Click(object sender, RoutedEventArgs e)
        {
            DATA0Entry entry = (DATA0Entry)dataGridDATA.SelectedItem;

            SaveFileDialog dialog = new SaveFileDialog { DefaultExt = ".bin.gz", FileName = entry.EntryID.ToString(), Filter = "GZipped FETH DATA0 Entry|*.bin.gz" };

            if (dialog.ShowDialog() == true)
            {
                using (EndianBinaryReader r = new EndianBinaryReader(new FileStream( data1Filepath, FileMode.Open ), Endianness.Little))
                {
                    using (EndianBinaryWriter w = new EndianBinaryWriter(new FileStream(dialog.FileName, FileMode.Create), Endianness.Little))
                    {
                        r.SeekBegin(entry.Offset);

                        w.Write( entry.Compressed ? r.ReadBytes( ( int ) entry.CompressedSize ) : r.ReadBytes( ( int ) entry.UncompressedSize ) );
                    }
                }
            }
        }

        private void ExtractDecompress_Click(object sender, RoutedEventArgs e)
        {
            DATA0Entry entry = (DATA0Entry)dataGridDATA.SelectedItem;

            SaveFileDialog dialog = new SaveFileDialog { DefaultExt = ".bin", FileName = entry.EntryID.ToString(), Filter = "FETH DATA0 Entry|*.bin" };

            if (entry.Compressed)
            {
                if (dialog.ShowDialog() == true)
                {
                    using (EndianBinaryReader r = new EndianBinaryReader(new FileStream( data1Filepath, FileMode.Open ), Endianness.Little))
                    {
                        using (EndianBinaryWriter w = new EndianBinaryWriter(new FileStream(dialog.FileName, FileMode.Create), Endianness.Little))
                        {
                            KTGZip zlib = new KTGZip();
                            r.SeekBegin(entry.Offset);
                            w.Write(zlib.Decompress(r.ReadBytes((int)entry.CompressedSize)));
                        }
                    }
                }
            }
        }

        private void ExtractAllButtonDATA_Click(object sender, RoutedEventArgs e)
        {

            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                foreach(DATA0Entry entry in data0)
                {
                    if (entry.CompressedSize == 0 || entry.UncompressedSize == 0)
                        continue;

                    string extension = entry.Compressed ? ".bin.gz" : ".bin";
                    using (EndianBinaryReader r = new EndianBinaryReader(new FileStream( data1Filepath, FileMode.Open ), Endianness.Little))
                    {
                        using (EndianBinaryWriter w = new EndianBinaryWriter(new FileStream(dialog.FileName + "/" + entry.EntryID + extension, FileMode.Create), Endianness.Little))
                        {
                            r.SeekBegin(entry.Offset);

                            w.Write( entry.Compressed ? r.ReadBytes( ( int ) entry.CompressedSize ) : r.ReadBytes( ( int ) entry.UncompressedSize ) );
                        }
                    }
                }
            }
        }

        private void DATAGrid_Drop(object sender, DragEventArgs e)
        {
            if ( e.Data.GetDataPresent( DataFormats.FileDrop ) )
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var file = files[0];                

                string data1Path = Path.GetDirectoryName(file) + "/DATA1.bin";

                data0.Read(file);

                if (File.Exists(data1Path))
                {
                    data1Filepath = data1Path;
                }
            }
            
        }

        private void DATAGrid_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
    }
}
