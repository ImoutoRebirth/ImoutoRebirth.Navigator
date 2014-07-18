﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ImoutoNavigator.Commands;
using ImagesDBLibrary.Model;
using System.Collections.ObjectModel;

namespace ImoutoNavigator.ViewModel
{
    class CollectionManagerVM : VMBase
    {
        #region Fields

        private ICommand _renameCommand;
        private ICommand _removeCommand;
        private ICommand _createCommand;

        #endregion Fields

        #region Constructors

        public CollectionManagerVM()
        {
            Reload();
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<CollectionVM> Collections { get; set; }

        public CollectionVM SelectedCollection { get; set; }

        #endregion Properties

        #region Methods

        public void Reload()
        {
            Collections = new ObservableCollection<CollectionVM>(CollectionM.Collections.Select(x => new CollectionVM(x)));
            OnPropertyChanged("Collections");
        }

        #endregion Methods

        #region Commands

        public ICommand RenameCommand
        {
            get
            {
                return _renameCommand ?? (_renameCommand = new RelayCommand(Rename, CanDoCollectionCommand));
            }
        }

        public ICommand RemoveCommand
        {
            get
            {
                return _removeCommand ?? (_removeCommand = new RelayCommand(Remove, CanDoCollectionCommand));
            }
        }

        public ICommand CreateCommand
        {
            get
            {
                return _createCommand ?? (_createCommand = new RelayCommand(Create));
            }
        }

        #endregion Commands

        #region Command Handlers

        private void Rename(object param)
        {
            if (SelectedCollection != null)
            {
                SelectedCollection.Rename(param as string);
                Reload();
            }
        }

        private void Remove(object param)
        {
            if (SelectedCollection != null)
            {
                SelectedCollection.Remove();
                Reload();
            }
        }

        private bool CanDoCollectionCommand(object param)
        {
            return SelectedCollection != null;
        }

        private void Create(object param)
        {
            try
            {
                CollectionM.Create("new collection");
                Reload();
            }
            catch
            {

            }
        }

        #endregion Command Handlers
    }
}
