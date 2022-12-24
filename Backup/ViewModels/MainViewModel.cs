using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backup.Librarys;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Collections;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using Backup.Models;
using FolderBrowserEx;
using System.IO;
using System.ComponentModel;
using Backup.Localization;
namespace Backup.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Fields

        long lenght = 0;
        private bool run_backup = false;
        private string adderss_backup = string.Empty;
        private bool error = false;
        List<BackupData> datas = new List<BackupData>();

        #endregion

        #region Properties

        private ObservableCollection<ListData> list_box = new ObservableCollection<ListData>();
        public ObservableCollection<ListData> ListDatas
        {
            get { return list_box; }
            set { list_box = value; SetPropertyChange(); }
        }

        private int progress_bar = 0;
        public int ProgressBarValue
        {
            get { return progress_bar; }
            set { progress_bar = value; SetPropertyChange(); }
        }

        private System.Windows.Visibility image_in = System.Windows.Visibility.Hidden;
        public System.Windows.Visibility ImageIn
        {
            get { return image_in; }
            set { image_in = value; SetPropertyChange(); }
        }

        private System.Windows.Visibility image_ok = System.Windows.Visibility.Hidden;
        public System.Windows.Visibility ImageOk
        {
            get { return image_ok; }
            set { image_ok = value; SetPropertyChange(); }
        }

        private System.Windows.Visibility progress = System.Windows.Visibility.Hidden;
        public System.Windows.Visibility ProgressBar
        {
            get { return progress; }
            set { progress = value; SetPropertyChange(); }
        }

        private string panel = "Panel App";
        public string Panel
        {
            get { return panel; }
            set { panel = value; SetPropertyChange(); }
        }

        #endregion

        #region Command 

        private ICommand close;
        public ICommand Close
        {
            get
            {
                if (close == null)
                    close = new RelayCommand(MethodClose, CanMethodClose);
                return close;
            }
        }
        private void MethodClose(object obj)
        {
            (obj as Window).Close();
        }
        private bool CanMethodClose(object obj)
        {
            return true;
        }

        private ICommand select;
        public ICommand Select
        {
            get
            {
                if (select == null)
                    select = new RelayCommand(SelectClose, CanSelectClose);
                return select;
            }
        }
        private void SelectClose(object obj)
        {
            ImageOk = Visibility.Hidden;
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Title = "Select a folder";
            folderBrowserDialog.InitialFolder = @"C:\";
            folderBrowserDialog.AllowMultiSelect = false;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                SetDirsFiles(folderBrowserDialog.SelectedFolder);
        }
        private bool CanSelectClose(object obj)
        {
            return true;
        }
        public void SetDirsFiles(string dir)
        {
            BackgroundWorker worker = new BackgroundWorker();
            ImageIn = Visibility.Visible;
            worker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e) { Completed(dir); };
            worker.DoWork += delegate (object sender, DoWorkEventArgs e) { RunService(dir); };
            worker.RunWorkerAsync();
        }
        public void Completed(string dir)
        {
            if (error == false)
            {
                ImageIn = Visibility.Hidden;
                ImageOk = Visibility.Visible;

                long values = lenght / 1024;
                string label = " KB ";

                if (values > 1024)
                {
                    values /= 1024;
                    label = " MB ";
                }

                if (values > 1024)
                {
                    values /= 1024;
                    label = " GB ";
                }

                DirectoryInfo directory = new DirectoryInfo(dir);
                ListDatas.Add(new ListData()
                {
                    Header = Path.GetFileName(dir),
                    Picture = "/Resources/DocumentsFolder.png",
                    Details = (values + label) + (" / Dirs : " + directory.GetDirectories("*", SearchOption.AllDirectories).Count() +
                    "  Files : " + directory.GetFiles("*.*", SearchOption.AllDirectories).Count())
                });
                values = 0;
            }
            else
            {
                error = false;
            }
        }
        public void RunService(string dir)
        {
            // Task : Edit add list dirs & files 
            try
            {
                lenght = 0;
                Panel = "Processing";
                DirectoryInfo directory = new DirectoryInfo(dir);

                foreach (var d in directory.GetDirectories("*", SearchOption.AllDirectories))
                {
                    datas.Add(new BackupData(d.FullName, new DirectoryInfo(d.FullName).LastAccessTime, 0, true));
                }

                foreach (var f in directory.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    long l = new FileInfo(f.FullName).Length;
                    datas.Add(new BackupData(f.FullName, new FileInfo(f.FullName).LastAccessTime, l, false));
                    lenght += l;
                }
                Panel = "Dirs : " + directory.GetDirectories("*", SearchOption.AllDirectories).Count() +
                    "  Files : " + directory.GetFiles("*.*", SearchOption.AllDirectories).Count();
            }
            catch (Exception exp)
            {
                error = true;
                ImageOk = Visibility.Hidden;
                ImageIn = Visibility.Hidden;
                Panel = "error : " + exp.Message;
            }
        }

        private ICommand clear;
        public ICommand Clear
        {
            get
            {
                if (clear == null)
                {
                    clear = new RelayCommand(MethodClear, CanMethodClear);
                }
                return clear;
            }
        }
        public void MethodClear(object obj)
        {
            ListDatas.Clear();
            datas.Clear();
            ImageIn = Visibility.Hidden;
            ImageOk = Visibility.Hidden;
            ProgressBar = Visibility.Hidden;
            Panel = "Panel App";
            run_backup = false;
            adderss_backup = string.Empty;
            error = false;
        }
        public bool CanMethodClear(object obj)
        {
            if (list_box.Count > 0 || datas.Count > 0)
                return true;
            return false;
        }

        private ICommand path_backup;
        public ICommand PathBackup
        {
            get
            {
                if (path_backup == null)
                {
                    path_backup = new RelayCommand(MethodPathBackup, CanMethodPathBackup);
                }
                return path_backup;
            }
        }
        public void MethodPathBackup(object obj)
        {
            ImageIn = Visibility.Hidden;
            ImageOk = Visibility.Hidden;
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Title = "Select a folder";
            folderBrowserDialog.InitialFolder = @"C:\";
            folderBrowserDialog.AllowMultiSelect = false;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                adderss_backup = folderBrowserDialog.SelectedFolder;
                Panel = "Adderss Backup : " + adderss_backup;
            }
        }
        public bool CanMethodPathBackup(object obj)
        {
            if (list_box.Count > 0)
                return true;
            return false;
        }

        private ICommand backup;
        public ICommand Backup
        {
            get
            {
                if (backup == null)
                    backup = new RelayCommand(MethodBackup, CanMethodBackup);
                return backup;
            }
        }
        public void MethodBackup(object obj)
        {
            // Task : Backup as datas - list datas , field bool run_backup
            run_backup = true;

            List<BackupData> dirs = datas.Where(x => x.dir == true).ToList();
            List<BackupData> files = datas.Where(x => x.dir == false).ToList();

            if (run_backup)
            {


            }
        }
        public bool CanMethodBackup(object obj)
        {
            if (list_box.Count > 0 && adderss_backup != string.Empty)
                return true;
            return false;
        }

        private ICommand backup_ai;
        public ICommand BackupAi
        {
            get
            {
                if (backup_ai == null)
                    backup_ai = new RelayCommand(MethodBackupAi, CanMethodBackupAi);
                return backup_ai;
            }
        }
        public void MethodBackupAi(object obj)
        {
            // Task : BackupAi as datas - list datas , field bool run_backup

        }
        public bool CanMethodBackupAi(object obj)
        {
            if (list_box.Count > 0 && adderss_backup != string.Empty)
                return true;
            return false;
        }

        private ICommand stop;
        public ICommand Stop
        {
            get
            {
                if (stop == null)
                    stop = new RelayCommand(MethodStop, CanMethodStop);
                return stop;
            }
        }
        public void MethodStop(object obj)
        {
            run_backup = false;
            ProgressBarValue = 0;
            ImageIn = Visibility.Hidden;
            ImageOk = Visibility.Hidden;
            ProgressBar = Visibility.Hidden;
            ListDatas.Clear();
            datas.Clear();
        }
        public bool CanMethodStop(object obj)
        {
            if (run_backup)
                return true;
            return false;
        }

        private ICommand education;
        public ICommand Education
        {
            get
            {
                if (education == null)
                    education = new RelayCommand(MethodEducation, CanMethodEducation);
                return education;
            }
        }
        public void MethodEducation(object obj)
        {
            MessageBox.Show(Texts.Education, "Education", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public bool CanMethodEducation(object obj) { return true; }

        private ICommand about;
        public ICommand About
        {
            get
            {
                if (about == null)
                    about = new RelayCommand(MethodAbout, CanMethodAbout);
                return about;
            }
        }
        public void MethodAbout(object obj)
        {
            MessageBox.Show(Texts.About, "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public bool CanMethodAbout(object obj) { return true; }

        #endregion
    }
}