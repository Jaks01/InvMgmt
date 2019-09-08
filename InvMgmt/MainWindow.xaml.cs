﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

using InvMgmt.Information.ViewModels;

namespace InvMgmt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public ObservableCollection<ItemViewModel> items { get; } = new ObservableCollection<ItemViewModel>();
        public FormViewModel form { get; set; } = new FormViewModel();
        public ItemFormViewModel itemForm { get; set; } = new ItemFormViewModel();
        public CategoryManagerViewModel categoryManager { get; } = new CategoryManagerViewModel();

		public SettingManager settingManager = new SettingManager();
		public MainWindow()
		{
			InitializeComponent();

			//setting loading
			settingManager.ReadFromSetting();

			//temp item start
			/*
            form.Id = "13123";
            form.Name = "dsfafdsfasf";
            form.Description = "dfjslkvja jdsflajvs afs";
			
            AddCategoryToList();
			
			itemForm.Name = "fdsvdnn";
            itemForm.Description = "fdsvsv";
            itemForm.Id = "1231";
            AddNewItemToCategory(categoryManager.Categories[0]);
			//temp item end
			
	*/
			//setting data contexts
			gridNewCat.DataContext = form;
			gridNewItem.DataContext = itemForm;
			gridExistingCat.DataContext = categoryManager;
			cbNewItemCategory.DataContext = categoryManager;
			tabInventory.DataContext = categoryManager;
			cmbChangeItemCategory.DataContext = categoryManager;
			tbSaveFolderPath.DataContext = settingManager.SaveFileManager;

			SaveDataHandler.InitializeConnection("data.sqlite", settingManager.SaveFileManager.FirstLaunch);
			if (settingManager.SaveFileManager.FirstLaunch)
				settingManager.NoLongerFirstLaunch();

			//SaveDataHandler.InitializeConnection("data.sqlite");
			//temp data
			//SaveDataHandler.InsertCategoryToTable(categoryManager.Categories[0]);
			//SaveDataHandler.CreateItemTable(categoryManager.Categories[0].Items[0]);
			AddCategoryToListDataBase();
			AddNewItemToCategoryFromDatabase();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			settingManager.WriteToSetting();
			RewriteAllData();
		}


		#region add menu category section
		private void BtnSubmitNewCat_Click(object sender, RoutedEventArgs e)
        {
			if (!form.IsAllFieldFull())
			{
				MessageBox.Show("All fields with asterisk must be filled.");
				return;
			}
			if (!form.IsIdValid(categoryManager))
			{
				MessageBox.Show("An item with the same ID already exists. ID must be unique.");
				return;
			}
            AddCategoryToList();
        }

        private void AddCategoryToList()
        {
            categoryManager.AddCategoryToList(form.GetCategory);
			SaveDataHandler.InsertCategoryToTable(form.GetCategory);
			SaveDataHandler.CreateItemTable(form.GetCategory.IdDb);
            form.Reset();
        }
		private void AddCategoryToListDataBase()
		{
			categoryManager.AddCategoryToListFromDatabase(SaveDataHandler.ReadCategoryTable());
		}

        private void BtnClearNewCat_Click(object sender, RoutedEventArgs e)
        {
            if (!form.IsOneFieldFilled())
                return;
            form.Reset();
        }

        private void BtnRemoveExistingCat_Click(object sender, RoutedEventArgs e)
        {
            if (dgExistingCat.SelectedItem == null)
                return;
			SaveDataHandler.RemoveCategoryInTable((CategoryViewModel)dgExistingCat.SelectedItem);
            categoryManager.RemoveCategoryFromList((CategoryViewModel)dgExistingCat.SelectedItem);
        }
		#endregion

		#region add menu item selection
		private void BtnSubmitNewItem_Click(object sender, RoutedEventArgs e)
        {
			Console.WriteLine("selected index " + cbNewItemCategory.SelectedIndex);

			if (cbNewItemCategory.SelectedIndex == -1 || !itemForm.CanAddItem || categoryManager.IsCategoryEmpty())
			{
				MessageBox.Show("All fields with asterisk must be filled.");
				return;
			}
			if (!itemForm.IsIdValid(categoryManager))
			{
				MessageBox.Show("An item with the same ID already exists. ID must be unique.");
				return;
			}
			AddNewItemToCategory((CategoryViewModel)cbNewItemCategory.SelectedItem);
            
        }
        private void AddNewItemToCategory(CategoryViewModel _cat)
        {
            categoryManager.AddNewItemToCategory(_cat, itemForm.GetItem);
			SaveDataHandler.InsertItemToTable(itemForm.GetItem);
			NewItemReset();
        }
		private void AddNewItemToCategoryFromDatabase()
		{
			for(int i = 0; i < categoryManager.CategoryCount; i++)
			{
				categoryManager.AddListItemToCategoryFromDatabase(i, SaveDataHandler.ReadItemTable(categoryManager.Categories[i].IdDb));
			}
		}

		private void BtnClearNewItem_Click(object sender, RoutedEventArgs e)
        {
            NewItemReset();
        }

        private void NewItemReset()
        {
            itemForm.Reset();
        }

        private void CmbSortByCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgItemList.Items.Refresh();
			categoryManager.RefreshInventoryItemCount();
		}

        private void TextBoxDoubleOnlyValidation_LostFocus(object sender, RoutedEventArgs e)
        {
			e.Handled = true;
			if (((TextBox)sender).Text.Length == 0)
            {
                ((TextBox)sender).Text = "0";
                return;
            }

            double temp;
            if (!double.TryParse(((TextBox)sender).Text, out temp))
            {
                ((TextBox)sender).Text = "0";
                MessageBox.Show("This field is number only");
                return;
            }
            if (temp < 0)
            {
                ((TextBox)sender).Text = "0";
                MessageBox.Show("This field cannot be negative");
                return;
            }
            ((TextBox)sender).Text = temp.ToString();
			((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
		}

        private void TextBoxIntOnlyValidation_LostFocus(object sender, RoutedEventArgs e)
        {
			if (((TextBox)sender).Text.Length == 0)
            {
                ((TextBox)sender).Text = "0";
                return;
            }

            int temp;
            if (!int.TryParse(((TextBox)sender).Text, out temp))
            {
                ((TextBox)sender).Text = "0";
                MessageBox.Show("This field is integer only");
				return;
            }
            if(temp < 0)
            {
                ((TextBox)sender).Text = "0";
                MessageBox.Show("This field cannot be negative");
				return;
            }
            ((TextBox)sender).Text = temp.ToString("N0");
			((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
		}

		#endregion

		#region item menu item list modification

		private void DgItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgItemList.SelectedItem == null)
                return;
            cmbChangeItemCategory.SelectedItem = categoryManager.FindCategoryUsingId(((ItemViewModel)dgItemList.SelectedItem).Category);
            
        }

        private void CmbChangeItemCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgItemList.SelectedItem == null)
                return;
            categoryManager.ChangeSingleItemCategory(((ItemViewModel)dgItemList.SelectedItem), ((CategoryViewModel)cmbChangeItemCategory.SelectedItem));
        }

		#endregion

		#region settings menu save file section


		private void BtnChangeFolder_Click(object sender, RoutedEventArgs e)
		{
			settingManager.SaveFileManager.SaveFolderPath = settingManager.GetPathFolderDialog;
		}

		#endregion

		#region database changes
		private void RewriteAllData()
		{
			SaveDataHandler.OpenConnection();
			for(int i = 0; i < categoryManager.CategoryCount; i++)
			{
				SaveDataHandler.UpdateCategoryInTableManualConnection(categoryManager.Categories[i]);
				for(int x = 0; x < categoryManager.Categories[i].Items.Count; x++)
				{
					SaveDataHandler.UpdateItemInTableManualConnection(categoryManager.Categories[i].Items[x]);
				}
			}
			SaveDataHandler.CloseConnection();
		}

		private void DgExistingCat_LostFocus(object sender, RoutedEventArgs e)
		{
			SaveDataHandler.OpenConnection();
			for (int i = 0; i < categoryManager.CategoryCount; i++)
				SaveDataHandler.UpdateCategoryInTableManualConnection(categoryManager.Categories[i]);

			SaveDataHandler.CloseConnection();
		}

		private void BtnInventoryDelete_Click(object sender, RoutedEventArgs e)
		{
			if (dgItemList.SelectedIndex == -1)
				return;
			SaveDataHandler.RemoveItemInTable((ItemViewModel)dgItemList.SelectedItem);
			categoryManager.Categories[categoryManager.CurrentCategoryIndex].RemoveItem((ItemViewModel)dgItemList.SelectedItem);
		}

		private void DgItemList_LostFocus(object sender, RoutedEventArgs e)
		{
			SaveDataHandler.OpenConnection();
			for(int i = 0; i < categoryManager.SelectedCategoryItems.Count; i++)
			{
				SaveDataHandler.UpdateItemInTableManualConnection(categoryManager.SelectedCategoryItems[i]);
			}
			SaveDataHandler.CloseConnection();
		}
		#endregion

		#region search
		private void BtnSearch_Click(object sender, RoutedEventArgs e)
		{
			SearchSystem s = new SearchSystem();
			categoryManager.SetCurrentItemToSeached(s.GetSearchedItem(tbItemSearch.Text, categoryManager.SelectedCategoryItems));
		}

		#endregion

		private void BtnClear_Click(object sender, RoutedEventArgs e)
		{
			tbItemSearch.Clear();
			categoryManager.SetCurrentItemAllToVisible();
		}
	}
}
