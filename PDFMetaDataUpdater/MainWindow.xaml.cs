using Microsoft.Win32;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Path = System.IO.Path;

namespace PDFMetaDataUpdater {
	public partial class MainWindow : Window {
		List<string> selectedFilePaths;

		#region Setup
		public MainWindow() {
			InitializeComponent();
			OpenPdfFileEditingStream();

			RefreshFilesSelectedMessage();
		}
		private static void OpenPdfFileEditingStream() {
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
		}
		#endregion

		OpenFileDialog openFileDialog = new OpenFileDialog();
		private void SelectPdfFiles(object sender, RoutedEventArgs e) {
			openFileDialog.Multiselect = true;
			openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
			if (openFileDialog.ShowDialog() == true) {
				selectedFilePaths = openFileDialog.FileNames.ToList<string>();
				RefreshFilesSelectedMessage();
			}
		}

		private void RunFileProcessing(object sender, RoutedEventArgs e) {
			if (selectedFilePaths == null) {
				return;
			}

			// Create new directory
			string directoryName = Path.GetDirectoryName(selectedFilePaths[0]);
			string newDirectoryName = directoryName + " NEW";
			if (Directory.Exists(newDirectoryName)) {
				Directory.Delete(newDirectoryName, true);
			}
			DirectoryInfo di = Directory.CreateDirectory(newDirectoryName);

			selectedFilePaths.Sort((s1, s2) => string.Compare(s1, s2));

			// Create updated pdf files in new directory
			int counter = 1;
			foreach (string filePath in selectedFilePaths) {
				PdfDocument document = PdfReader.Open(filePath);
				//string oldName = Path.GetFileName(filePath);
				//string oldTitle = document.Info.Title;

				string volumeNumber = counter.ToString().PadLeft(3, '0');
				string newName = "One Piece v" + volumeNumber + ".pdf";
				string newTitle = "One Piece v" + volumeNumber;
				counter++;

				document.Info.Title = newTitle;
				document.Info.Author = "Oda";
				document.Save(newDirectoryName + "/" + newName);
			}
		}




		private void RefreshFilesSelectedMessage() {
			int numFilesSelected = (selectedFilePaths == null) ? 0 : selectedFilePaths.Count;
			FilesSelectedMessage.Text = numFilesSelected + " files selected";
		}
	}
}
