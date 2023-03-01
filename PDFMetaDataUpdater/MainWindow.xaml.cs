using Microsoft.Win32;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace PDFMetaDataUpdater {
	public partial class MainWindow : Window {
		List<string> selectedFilePaths;

		string customMangaTitle = "MangaTitle";
		string customMangaAuthor = "MangaAuthor";
		int custom0sPadding = 2;

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
			TitleField.Text = customMangaTitle;
			AuthorField.Text = customMangaAuthor;
			Padding0sSlider.Value = custom0sPadding;
		}

		private static void OpenPdfFileEditingStream() {
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
		}
		#endregion


		#region Open Files Dialog
		OpenFileDialog openFileDialog = new OpenFileDialog();
		private void SelectPdfFiles(object sender, RoutedEventArgs e) {
			openFileDialog.Multiselect = true;
			openFileDialog.Title = "Select manga volumes!";
			openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";

			if (openFileDialog.ShowDialog() == true) {
				selectedFilePaths = openFileDialog.FileNames.ToList<string>();
				RefreshFilesSelectedMessage();
			}
		}
		#endregion


		#region Editing Metadata Formatting
		private void ApplyFormattingChanges(object sender, RoutedEventArgs e) {
			customMangaTitle = TitleField.Text;
			customMangaAuthor = AuthorField.Text;
			custom0sPadding = (int)Padding0sSlider.Value;
			RefreshNewFormattingPreview();
		}
		#endregion


		#region Running
		private void RunFileProcessing(object sender, RoutedEventArgs e) {
			if (selectedFilePaths == null) {
				return;
			}

			selectedFilePaths.Sort((s1, s2) => string.Compare(s1, s2));

			string newDirectoryName = CreateNewDirectory();
			CreateUpdatedFilesInNewDirectory(newDirectoryName);
		}

		private string CreateNewDirectory() {
			string directoryName = Path.GetDirectoryName(selectedFilePaths[0]);
			string newDirectoryName = directoryName + " NEW";
			if (Directory.Exists(newDirectoryName)) {
				Directory.Delete(newDirectoryName, true);
			}
			DirectoryInfo di = Directory.CreateDirectory(newDirectoryName);
			return newDirectoryName;
		}

		private async void CreateUpdatedFilesInNewDirectory(string newDirectory) {
			DisableRunButton();

			int counter = 1;
			foreach (string filePath in selectedFilePaths) {
				RefreshingRunningProgressMessage(counter, selectedFilePaths.Count);

				await Task.Delay(1);

				string volumeNumber = counter.ToString().PadLeft(custom0sPadding, '0');
				string newName = customMangaTitle + " v" + volumeNumber + ".pdf";
				string newTitle = customMangaTitle + " v" + volumeNumber;

				PdfDocument document = PdfReader.Open(filePath);
				document.Info.Title = newTitle;
				document.Info.Author = customMangaAuthor;
				document.Save(newDirectory + "/" + newName);

				counter++;
			}

			ClearRunningProgressMessage();

			EnableRunButton();
		}
		#endregion


		#region Refreshing Frontend
		private void RefreshFilesSelectedMessage() {
			int numFilesSelected = (selectedFilePaths == null) ? 0 : selectedFilePaths.Count;
			FilesSelectedMessage.Text = numFilesSelected + " files selected";
		}

		private void RefreshNewFormattingPreview() {
			string padding0sPreview = "";
			for (int p = 0; p < custom0sPadding; p++) {
				padding0sPreview += '#';
			}
			NewFormattingPreviewTitle.Text = "Title: " + customMangaTitle + " v" + padding0sPreview;

			NewFormattingPreviewAuthor.Text = "Author: " + customMangaAuthor;
		}

		private void RefreshingRunningProgressMessage(int curDocument, int totalDocuments) {
			RunningProgressMessage.Text = "Processing " + curDocument + " / " + totalDocuments + "...";
		}
		private void ClearRunningProgressMessage() {
			RunningProgressMessage.Text = "";
		}

		private void EnableRunButton() => RunButton.IsEnabled = true;
		private void DisableRunButton() => RunButton.IsEnabled = false;
		#endregion

		private void SelectAddress(object sender, RoutedEventArgs e) {
			TextBox tb = (sender as TextBox);
			tb?.SelectAll();
		}
	}
}
