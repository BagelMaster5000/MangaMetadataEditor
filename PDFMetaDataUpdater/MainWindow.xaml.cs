using Microsoft.Win32;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Path = System.IO.Path;

namespace PDFMetaDataUpdater {
	public partial class MainWindow : Window {
		List<string> selectedFilePaths;

		string curMangaTitle = "MangaTitle";
		string curMangaAuthor = "MangaAuthor";
		int curPadding0s = 2;

		#region Setup
		public MainWindow() {
			InitializeComponent();
			OpenPdfFileEditingStream();
			InitializeInputFields();

			RefreshFilesSelectedMessage();
			RefreshNewFormattingPreview();
			ClearRunningProgressMessage();
			EnableRunButton();

		}

		private void InitializeInputFields() {
			TitleField.Text = curMangaTitle;
			AuthorField.Text = curMangaAuthor;
			Padding0sSlider.Value = curPadding0s;
		}

		private static void OpenPdfFileEditingStream() {
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
		}
		#endregion


		#region Open Files Dialog
		OpenFileDialog openFileDialog = new OpenFileDialog();
		private void SelectPdfFiles(object sender, RoutedEventArgs e) {
			openFileDialog.Multiselect = true;
			openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
			if (openFileDialog.ShowDialog() == true) {
				selectedFilePaths = openFileDialog.FileNames.ToList<string>();
				RefreshFilesSelectedMessage();
			}
		}
		#endregion


		#region Input Fields
		private void SaveEnteredTitle(object sender, RoutedEventArgs e) {
			curMangaTitle = TitleField.Text;
			RefreshNewFormattingPreview();
		}
		private void SaveEnteredAuthor(object sender, RoutedEventArgs e) {
			curMangaAuthor = AuthorField.Text;
			RefreshNewFormattingPreview();
		}
		private void SaveEnteredPadding0s(object sender, RoutedEventArgs e) {
			curPadding0s = (int)Padding0sSlider.Value;
			RefreshNewFormattingPreview();
		}
		#endregion


		#region Running
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

			CreateUpdatedFilesInNewDirectory(newDirectoryName);

			// Create updated pdf files in new directory
			//int counter = 1;
			//foreach (string filePath in selectedFilePaths) {
			//	RefreshingRunningProgressMessage(counter, selectedFilePaths.Count);

			//	string volumeNumber = counter.ToString().PadLeft(3, '0');
			//	string newName = "One Piece v" + volumeNumber + ".pdf";
			//	string newTitle = "One Piece v" + volumeNumber;

			//	PdfDocument document = PdfReader.Open(filePath);
			//	document.Info.Title = newTitle;
			//	document.Info.Author = "Oda";
			//	document.Save(newDirectoryName + "/" + newName);

			//	counter++;
			//}

			//ClearRunningProgressMessage();
		}

		private async void CreateUpdatedFilesInNewDirectory(string newDirectory) {
			DisableRunButton();

			int counter = 1;
			foreach (string filePath in selectedFilePaths) {
				RefreshingRunningProgressMessage(counter, selectedFilePaths.Count);

				string volumeNumber = counter.ToString().PadLeft(curPadding0s, '0');
				string newName = curMangaTitle + " v" + volumeNumber + ".pdf";
				string newTitle = curMangaTitle + " v" + volumeNumber;

				PdfDocument document = PdfReader.Open(filePath);
				document.Info.Title = newTitle;
				document.Info.Author = curMangaAuthor;
				document.Save(newDirectory + "/" + newName);

				counter++;
				await Task.Delay(5);
			}

			ClearRunningProgressMessage();

			EnableRunButton();
		}

		#endregion


		private void EnableRunButton() => RunButton.IsEnabled = true;
		private void DisableRunButton() => RunButton.IsEnabled = false;

		#region Refreshing frontend
		private void RefreshFilesSelectedMessage() {
			int numFilesSelected = (selectedFilePaths == null) ? 0 : selectedFilePaths.Count;
			FilesSelectedMessage.Text = numFilesSelected + " files selected";
		}

		private void RefreshNewFormattingPreview() {
			string padding0sPreview = "";
			for (int p = 0; p < curPadding0s; p++) {
				padding0sPreview += '#';
			}
			NewFormattingPreviewTitle.Text = "Title: " + curMangaTitle + " v" + padding0sPreview;

			NewFormattingPreviewAuthor.Text = "Author: " + curMangaAuthor;
		}

		private void RefreshingRunningProgressMessage(int curDocument, int totalDocuments) {
			RunningProgressMessage.Text = "Processing " + curDocument + " / " + totalDocuments + "...";
		}
		private void ClearRunningProgressMessage() {
			RunningProgressMessage.Text = "";
		}
		#endregion
	}
}
