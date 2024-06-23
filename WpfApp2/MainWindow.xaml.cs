using System.Net.Http;
using System.Net.Http.Formatting;
using Client.Models;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Net;


namespace Client
{
    public partial class MainWindow : Window
    {
        HttpClient client = new HttpClient();

        public MainWindow()
        {

            InitializeComponent();
            client.BaseAddress = new Uri("http://localhost:5138/");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private async void btnLoadStudents_Click(object sender, RoutedEventArgs eventArgs)
        {
            LoadEducationData();
            LoadCompositionFamilyData();


            try
            {
                var response = await client.GetStringAsync("student");
                var educationResponse = await client.GetStringAsync("education");
                var familyresponse = await client.GetStringAsync("Composition_Family");



                var students = JsonConvert.DeserializeObject<List<Student>>(response);
                var educationList = JsonConvert.DeserializeObject<List<Education>>(educationResponse);
                var compositionList = JsonConvert.DeserializeObject<List<Composition_Family>>(familyresponse);

                foreach (var student in students)
                {
                    var education = educationList.FirstOrDefault(e => e.educationID == student.educationID);
                    if (education != null)
                    {
                        student.EducationInstitutionName = education.name_of_education_institution;
                    }
                    var composition = compositionList.FirstOrDefault(c => c.compositionID == student.compositionID);
                    if (composition != null)
                    {
                        student.CompositionName = composition.family_completeness;
                    }
                }


                dgStudents.ItemsSource = students;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void LoadEducationData()
        {
            try
            {
                var response = await client.GetAsync("education");
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var educationList = JsonConvert.DeserializeObject<List<Education>>(jsonString);

                var educationNames = educationList.Select(education => education.name_of_education_institution).ToList();

                cmbEducation.ItemsSource = educationNames;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных об образовании: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void LoadCompositionFamilyData()
        {
            try
            {
                var response = await client.GetAsync("Composition_Family");
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var compositionFamilyList = JsonConvert.DeserializeObject<List<Composition_Family>>(jsonString);

                var familyCompositions = compositionFamilyList.Select(composition => composition.family_completeness).ToList();

                cmbFamilyComposition.ItemsSource = familyCompositions;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных о составе семьи: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<List<Education>> LoadEducationDataAsync()
        {
            try
            {
                var response = await client.GetAsync("education");
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var educationList = JsonConvert.DeserializeObject<List<Education>>(jsonString);

                return educationList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных об образовании: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        private async Task<List<Composition_Family>> LoadCompositionFamilyDataAsync()
        {
            try
            {
                var response = await client.GetAsync("Composition_Family");
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var compositionFamilyList = JsonConvert.DeserializeObject<List<Composition_Family>>(jsonString);

                return compositionFamilyList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных о составе семьи: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        private async void btnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newStudent = new Student
                {
                    First_Name = txtLastName.Text,
                    middle_name = txtFirstName.Text,
                    last_name = txtMiddleName.Text,
                    birthday_date = dpBirthdayDate.SelectedDate ?? DateTime.MinValue,
                    educationID = cmbEducation.SelectedIndex + 1, // +1 if IDs start from 1, adjust accordingly
                    compositionID = cmbFamilyComposition.SelectedIndex + 1,
                    LivingAddress = txtAddressLiving.Text,
                    Registration_start_date = dpRegistrationDateStart.SelectedDate ?? DateTime.MinValue
                };

                var json = JsonConvert.SerializeObject(newStudent);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("Student", content);
                response.EnsureSuccessStatusCode();
                ClearStudentFields();
                MessageBox.Show("Студент добавлен.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                btnLoadStudents_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не все поля заполнены: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void btnDeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            if (dgStudents.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите студента для удаления.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selectedStudent = dgStudents.SelectedItem as Student; // Предполагаем, что у вас есть класс Student
            if (selectedStudent != null)
            {
                try
                {
                    var response = await client.DeleteAsync($"student/{selectedStudent.studentID}");
                    response.EnsureSuccessStatusCode();
                    MessageBox.Show("Студент удалён", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    btnLoadStudents_Click(sender, e); // Перезагрузите данные студентов в DataGrid после удаления
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void DataGridStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgStudents.SelectedItem is Student selectedStudent)
            {

                txtFirstName.Text = selectedStudent.First_Name;
                txtMiddleName.Text = selectedStudent.middle_name;
                txtLastName.Text = selectedStudent.last_name;
                dpBirthdayDate.SelectedDate = selectedStudent.birthday_date;
                cmbEducation.Text = selectedStudent.EducationInstitutionName; // Установите текст в ComboBox на наименование учреждения образования
                cmbFamilyComposition.Text = selectedStudent.CompositionName;
                txtAddressLiving.Text = selectedStudent.LivingAddress;
                dpRegistrationDateStart.SelectedDate = selectedStudent.Registration_start_date;
            }
        }

        private async void btnUpdateStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgStudents.SelectedItem is Student selectedStudent)
                {
                    selectedStudent.First_Name = txtFirstName.Text;
                    selectedStudent.middle_name = txtMiddleName.Text;
                    selectedStudent.last_name = txtLastName.Text;
                    selectedStudent.birthday_date = dpBirthdayDate.SelectedDate ?? DateTime.MinValue;

                    // Поиск соответствующего ID образования по выбранному наименованию
                    var selectedEducation = cmbEducation.Text;
                    var educationList = await LoadEducationDataAsync();

                    var education = educationList.FirstOrDefault(ed => ed.name_of_education_institution == selectedEducation);
                    selectedStudent.educationID = education != null ? education.educationID : 0;

                    var selectedComposition = cmbFamilyComposition.Text;
                    var compList = await LoadCompositionFamilyDataAsync();
                    var composition = compList.FirstOrDefault(comp => comp.family_completeness == selectedComposition);
                    // Используем выбранное значение из cmbFamilyComposition вместо txtCompositionId
                    selectedStudent.compositionID = composition != null ? composition.compositionID : 0;

                    selectedStudent.LivingAddress = txtAddressLiving.Text;
                    selectedStudent.Registration_start_date = dpRegistrationDateStart.SelectedDate ?? DateTime.MinValue;

                    var json = JsonConvert.SerializeObject(selectedStudent);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync($"student/{selectedStudent.studentID}", content);
                    response.EnsureSuccessStatusCode();
                    ClearStudentFields();
                    MessageBox.Show("Обновление успешно", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    btnLoadStudents_Click(sender, e); // Перезагрузите данные студентов в DataGrid после обновления
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите студента для обновления.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Метод для загрузки данных об образовании из базы данных




        private void ClearStudentFields()
        {

            txtFirstName.Clear();
            txtMiddleName.Clear();
            txtLastName.Clear();

            txtAddressLiving.Clear();
            dpBirthdayDate.SelectedDate = null;
            dpRegistrationDateStart.SelectedDate = null;
        }
        private void stuFilt(object sender, RoutedEventArgs e)
        {
            string filterText = stfilt.Text.Trim();
            ICollectionView view = CollectionViewSource.GetDefaultView(dgStudents.ItemsSource);
            if (!string.IsNullOrWhiteSpace(filterText))
            {
                view.Filter = item => ((Student)item).First_Name.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            else
            {
                view.Filter = null;
            }
        }
        private void stuSort(object sender, RoutedEventArgs e)
        {
            // Получение выбранного элемента ComboBox и его преобразование в строку
            string sortBy = ((ComboBoxItem)cmbSortBystd.SelectedItem).Content.ToString();
            ICollectionView view = CollectionViewSource.GetDefaultView(dgStudents.ItemsSource);

            // Очистка существующих описаний сортировки
            view.SortDescriptions.Clear();

            // Применение сортировки в зависимости от выбранного элемента ComboBox
            if (sortBy == "Ascending")
            {
                view.SortDescriptions.Add(new SortDescription("last_name", ListSortDirection.Ascending));
            }
            else if (sortBy == "Descending")
            {
                view.SortDescriptions.Add(new SortDescription("last_name", ListSortDirection.Descending));
            }
            else if (sortBy == "No Sorting")
            {
                view.SortDescriptions.Clear();
            }
            // Если выбран "No Sorting", сортировки не применяются (view.SortDescriptions уже очищен)
        }
        private void stuSearch(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearchSt.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                dgStudents.UnselectAll();
                return;
            }

            dgStudents.UpdateLayout();

            foreach (var item in dgStudents.Items)
            {
                dgStudents.ScrollIntoView(item);
                DataGridRow row = (DataGridRow)dgStudents.ItemContainerGenerator.ContainerFromItem(item);

                if (row != null)
                {
                    if (item is Student student)
                    {
                        bool matchesSearch = false;

                        if (student.First_Name?.ToLower().Contains(searchText) == true ||
                            student.middle_name?.ToLower().Contains(searchText) == true ||
                            student.last_name?.ToLower().Contains(searchText) == true ||
                            student.birthday_date.ToString("dd.MM.yyyy").ToLower().Contains(searchText) == true ||
                            student.EducationInstitutionName?.ToLower().Contains(searchText) == true ||
                            student.CompositionName?.ToLower().Contains(searchText) == true ||
                            student.LivingAddress?.ToLower().Contains(searchText) == true ||
                            student.Registration_start_date.ToString("dd.MM.yyyy").ToLower().Contains(searchText) == true)
                        {
                            matchesSearch = true;
                        }

                        row.Background = matchesSearch ? Brushes.Yellow : Brushes.White; // Подсветка строки с соответствующим значением
                    }
                }
            }
        }






        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private async void btnLoadEducation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = await client.GetStringAsync("education");
                var educationList = JsonConvert.DeserializeObject<List<Education>>(response);
                dgEducation.ItemsSource = educationList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void btnAddEducation_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                var newEducation = new Education
                {

                    name_of_education_institution = txtNameOfEducationInstitution.Text,
                    address_of_educational_institution = txtAddressOfEducationalInstitution.Text,
                    endDate = dpEndDate.SelectedDate ?? DateTime.MinValue
                };

                var json = JsonConvert.SerializeObject(newEducation);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("education", content);
                response.EnsureSuccessStatusCode();



                txtNameOfEducationInstitution.Clear();
                txtAddressOfEducationalInstitution.Clear();
                dpEndDate.SelectedDate = null;

                MessageBox.Show("Образование добавлено успешно", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                btnLoadEducation_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnDeleteEducation_Click(object sender, RoutedEventArgs e)
        {
            if (dgEducation.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите образовательное учреждение для удаления.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var selectedEducation = dgEducation.SelectedItem as Education; // Предполагаем, что у вас есть класс Student
            if (selectedEducation != null)
            {
                try
                {
                    var response = await client.DeleteAsync($"Education/{selectedEducation.educationID}");
                    response.EnsureSuccessStatusCode();
                    MessageBox.Show("Студент удалён", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    btnLoadEducation_Click(sender, e);  // Перезагрузите данные студентов в DataGrid после удаления
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void ClearEducationFields()
        {

            txtNameOfEducationInstitution.Clear();
            txtAddressOfEducationalInstitution.Clear();
            dpEndDate.SelectedDate = null;
        }
        private async void btnUpdateEducation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgEducation.SelectedItem is Education selectedEducation)
                {
                    selectedEducation.name_of_education_institution = txtNameOfEducationInstitution.Text;
                    selectedEducation.address_of_educational_institution = txtAddressOfEducationalInstitution.Text;
                    selectedEducation.endDate = dpEndDate.SelectedDate ?? DateTime.MinValue;

                    var json = JsonConvert.SerializeObject(selectedEducation);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync($"education/{selectedEducation.educationID}", content);
                    response.EnsureSuccessStatusCode();

                    ClearEducationFields();
                    MessageBox.Show("Образование успешно обновлено", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    btnLoadEducation_Click(sender, e); // Перезагрузите данные об образовании в DataGrid после обновления
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите запись для обновления.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DataGridEducation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgEducation.SelectedItem is Education selectedEducation)
            {

                txtNameOfEducationInstitution.Text = selectedEducation.name_of_education_institution;
                txtAddressOfEducationalInstitution.Text = selectedEducation.address_of_educational_institution;
                dpEndDate.SelectedDate = selectedEducation.endDate;
            }
        }
        private void edSort(object sender, RoutedEventArgs e)
        {
            string sortBy = ((ComboBoxItem)cmbSortByed.SelectedItem).Content.ToString();
            ICollectionView view = CollectionViewSource.GetDefaultView(dgEducation.ItemsSource);

            if (sortBy == "Ascending")
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("name_of_education_institution", ListSortDirection.Ascending));
            }
            else if (sortBy == "Descending")
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("name_of_education_institution", ListSortDirection.Descending));
            }
            else if (sortBy == "No Sorting")
            {
                view.SortDescriptions.Clear();
            }
        }
        private void edFilt(object sender, RoutedEventArgs e)
        {
            string filterText = edfilt.Text.Trim();
            ICollectionView view = CollectionViewSource.GetDefaultView(dgEducation.ItemsSource);

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                view.Filter = item => ((Education)item).name_of_education_institution.Contains(filterText);
            }
            else
            {
                view.Filter = null;
            }
        }
        private void edSearch(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearched.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                dgEducation.UnselectAll();
                return;
            }

            foreach (var item in dgEducation.Items)
            {
                DataGridRow row = (DataGridRow)dgEducation.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                {
                    if (item is Education education)
                    {
                        if (education.name_of_education_institution.ToLower().Contains(searchText) ||
                            education.address_of_educational_institution.ToLower().Contains(searchText) ||
                            education.endDate.ToString("dd.MM.yyyy").ToLower().Contains(searchText))
                        {
                            row.Background = Brushes.Yellow; // Подсветка строки с соответствующим значением
                        }
                        else
                        {
                            row.Background = Brushes.White; // Возврат исходного цвета для остальных строк
                        }
                    }
                }
            }
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool phoneErrorDisplayed = false;

        private void txtWorkPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            // Check if input text is a digit
            return text.All(char.IsDigit);
        }

        private void txtWorkPhone_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!phoneErrorDisplayed && txtWorkPhone.Text.Length != 11)
            {
                MessageBox.Show("Номер телефона должен состоять из 11 цифр", "Неверный номер телефона", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtWorkPhone.Focus();
                phoneErrorDisplayed = true; // Set the flag to true to prevent further error messages
            }
        }

        private async void btnLoadParents_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = await client.GetStringAsync("parents");
                var parents = JsonConvert.DeserializeObject<List<Parents>>(response);
                dgParents.ItemsSource = parents;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void btnAddParent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtWorkPhone.Text.Length != 11)
                {
                    MessageBox.Show("Номер телефона должен состоять из 11 цифр", "Неверный номер телефона", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtWorkPhone.Focus();
                    return;
                }

                var newParent = new Parents
                {
                    studentID = int.Parse(txtStudentID_Parent.Text),
                    First_Name = txtFirstName_Parent.Text,
                    Last_Name = txtLastName_Parent.Text,
                    Middle_Name = txtMiddleName_Parent.Text,
                    jobPlace = txtJobPlace.Text,
                    job_Title = txtJobTitle.Text,
                    Work_phone = txtWorkPhone.Text,
                    Living_adress = txtLivingAddress.Text
                };

                var json = JsonConvert.SerializeObject(newParent);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("parents", content);
                response.EnsureSuccessStatusCode();
                ClearParentFields();
                MessageBox.Show("Родитель добавлен.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                btnLoadParents_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnUpdateParent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgParents.SelectedItem is Parents selectedParent)
                {
                    selectedParent.studentID = int.Parse(txtStudentID_Parent.Text);
                    selectedParent.First_Name = txtFirstName_Parent.Text;
                    selectedParent.Last_Name = txtLastName_Parent.Text;
                    selectedParent.Middle_Name = txtMiddleName_Parent.Text;
                    selectedParent.jobPlace = txtJobPlace.Text;
                    selectedParent.job_Title = txtJobTitle.Text;
                    selectedParent.Work_phone = txtWorkPhone.Text;
                    selectedParent.Living_adress = txtLivingAddress.Text;

                    var json = JsonConvert.SerializeObject(selectedParent);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync($"parents/{selectedParent.parentID}", content);
                    response.EnsureSuccessStatusCode();
                    ClearParentFields();
                    MessageBox.Show("Родитель обновлён успешно", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    btnLoadParents_Click(sender, e);
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите родителя для обновления.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ClearParentFields()
        {

            txtStudentID_Parent.Clear();
            txtFirstName_Parent.Clear();
            txtLastName_Parent.Clear();
            txtMiddleName_Parent.Clear();
            txtJobPlace.Clear();
            txtJobTitle.Clear();
            txtWorkPhone.Clear();
            txtLivingAddress.Clear();
        }
        private void DataGridParents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgParents.SelectedItem is Parents selectedParent)
            {

                txtStudentID_Parent.Text = selectedParent.studentID.ToString();
                txtFirstName_Parent.Text = selectedParent.First_Name;
                txtLastName_Parent.Text = selectedParent.Last_Name;
                txtMiddleName_Parent.Text = selectedParent.Middle_Name;
                txtJobPlace.Text = selectedParent.jobPlace;
                txtJobTitle.Text = selectedParent.job_Title;
                txtWorkPhone.Text = selectedParent.Work_phone;
                txtLivingAddress.Text = selectedParent.Living_adress;
            }
        }
        private async void btnDeleteParent_Click(object sender, RoutedEventArgs e)
        {
            if (dgParents.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите родителя для удаления.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var selectedParent = dgParents.SelectedItem as Parents;
            if (selectedParent != null)
            {
                try
                {
                    var response = await client.DeleteAsync($"parents/{selectedParent.parentID}");
                    response.EnsureSuccessStatusCode();
                    MessageBox.Show("Родитель удалён", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    btnLoadParents_Click(sender, e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void paSearch(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearchedpa.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                dgParents.UnselectAll();
                return;
            }

            foreach (var item in dgParents.Items)
            {
                DataGridRow row = (DataGridRow)dgParents.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                {
                    if (item is Parents parent)
                    {
                        if (parent.First_Name.ToLower().Contains(searchText) ||
                            parent.Last_Name.ToLower().Contains(searchText) ||
                            parent.Middle_Name.ToLower().Contains(searchText) ||

                            parent.jobPlace.ToLower().Contains(searchText) ||
                            parent.job_Title.ToLower().Contains(searchText) ||
                            parent.Work_phone.ToLower().Contains(searchText) ||
                            parent.Living_adress.ToLower().Contains(searchText))
                        {
                            row.Background = Brushes.Yellow; // Подсветка строки с соответствующим значением
                        }
                        else
                        {
                            row.Background = Brushes.White; // Возврат исходного цвета для остальных строк
                        }
                    }
                }
            }
        }
        private void paFilt(object sender, RoutedEventArgs e)
        {
            string filterText = pafilt.Text.Trim();
            ICollectionView view = CollectionViewSource.GetDefaultView(dgParents.ItemsSource);

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                view.Filter = item => ((Parents)item).First_Name.Contains(filterText);
            }
            else
            {
                view.Filter = null;
            }
        }
        private void paSort(object sender, RoutedEventArgs e)
        {
            string sortBy = ((ComboBoxItem)cmbSortBypa.SelectedItem).Content.ToString();
            ICollectionView view = CollectionViewSource.GetDefaultView(dgParents.ItemsSource);

            if (sortBy == "Ascending")
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("First_Name", ListSortDirection.Ascending));
            }
            else if (sortBy == "Descending")
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("First_Name", ListSortDirection.Descending));
            }
            else if (sortBy == "No Sorting")
            {
                view.SortDescriptions.Clear();
            }
        }




        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        private async void btnLoadFamilies_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = await client.GetStringAsync("Composition_Family");
                var families = JsonConvert.DeserializeObject<List<Composition_Family>>(response);
                dgFamilies.ItemsSource = families;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnAddFamily_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                var newFamily = new Composition_Family
                {

                    family_completeness = txtFamilyCompleteness.Text
                };

                var json = JsonConvert.SerializeObject(newFamily);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("Composition_Family", content);
                response.EnsureSuccessStatusCode();
                ClearFamilyFields();
                MessageBox.Show("Состав семьи добавлен", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                btnLoadFamilies_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataGridfamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgFamilies.SelectedItem is Composition_Family selectedFamily)
            {

                txtFamilyCompleteness.Text = selectedFamily.family_completeness;
            }
        }

        private async void btnUpdateFamily_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgFamilies.SelectedItem is Composition_Family selectedFamily)
                {
                    selectedFamily.family_completeness = txtFamilyCompleteness.Text;

                    var json = JsonConvert.SerializeObject(selectedFamily);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync($"Composition_Family/{selectedFamily.compositionID}", content);
                    response.EnsureSuccessStatusCode();
                    ClearFamilyFields();
                    MessageBox.Show("Состав семьи успешно обновлен", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    btnLoadFamilies_Click(sender, e); // Перезагрузите данные семей в DataGrid после обновления
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите семью для обновления.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void btnDeleteFamily_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgFamilies.SelectedItem is Composition_Family selectedFamily)
                {
                    var response = await client.DeleteAsync($"Composition_Family/{selectedFamily.compositionID}");
                    response.EnsureSuccessStatusCode();
                    MessageBox.Show("Состав семьи успешно удалён", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    btnLoadFamilies_Click(sender, e); // Перезагрузите данные семей в DataGrid после удаления
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите семью для удаления.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void faSort(object sender, RoutedEventArgs e)
        {
            string sortBy = ((ComboBoxItem)cmbSortByfa.SelectedItem).Content.ToString();
            ICollectionView view = CollectionViewSource.GetDefaultView(dgFamilies.ItemsSource);

            if (sortBy == "Ascending")
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("family_completeness", ListSortDirection.Ascending));
            }
            else if (sortBy == "Descending")
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("family_completeness", ListSortDirection.Descending));
            }
            else if (sortBy == "No Sorting")
            {
                view.SortDescriptions.Clear();
            }
        }

        private void faFilt(object sender, RoutedEventArgs e)
        {
            string filterText = fafilt.Text.Trim().ToLower();
            ICollectionView view = CollectionViewSource.GetDefaultView(dgFamilies.ItemsSource);

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                view.Filter = item => ((Composition_Family)item).family_completeness.ToLower().Contains(filterText);
            }
            else
            {
                view.Filter = null;
            }
        }

        private void faSearch(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearchedfa.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                dgFamilies.UnselectAll();
                return;
            }

            dgFamilies.UpdateLayout();

            foreach (var item in dgFamilies.Items)
            {
                dgFamilies.ScrollIntoView(item);
                DataGridRow row = (DataGridRow)dgFamilies.ItemContainerGenerator.ContainerFromItem(item);

                if (row != null)
                {
                    if (item is Composition_Family family)
                    {
                        if (family.family_completeness.ToLower().Contains(searchText))
                        {
                            row.Background = Brushes.Yellow; // Подсветка строки с соответствующим значением
                        }
                        else
                        {
                            row.Background = Brushes.White; // Возврат исходного цвета для остальных строк
                        }
                    }
                }
            }
        }



        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private async void LoadParticipation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var participationResponse = await client.GetStringAsync("Participation_in_event");
                var studentResponse = await client.GetStringAsync("Student");
                var eventResponse = await client.GetStringAsync("Eventss");

                var students = JsonConvert.DeserializeObject<List<Student>>(studentResponse);
                var events = JsonConvert.DeserializeObject<List<Eventss>>(eventResponse);
                var participations = JsonConvert.DeserializeObject<List<Participation_in_event>>(participationResponse);

                // Загрузка данных в комбобоксы
                LoadStudents(students);
                LoadEvents(events);

                // Проход по каждому Participation_in_event и нахождение соответствующего Student и Eventss
                foreach (var participation in participations)
                {
                    // Находим студента по studentID
                    participation.Student = students.FirstOrDefault(s => s.studentID == participation.studentID);

                    // Находим событие по eventID
                    participation.Eventss = events.FirstOrDefault(ev => ev.eventID == participation.eventID);
                }

                // Установка источника данных для DataGrid
                dgParticipation.ItemsSource = participations;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private async void AddParticipation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var studentResponse = await client.GetStringAsync("Student");
                var eventResponse = await client.GetStringAsync("Eventss");

                var students = JsonConvert.DeserializeObject<List<Student>>(studentResponse);
                var events = JsonConvert.DeserializeObject<List<Eventss>>(eventResponse);

                var selectedStudent = cmbStudent.SelectedItem as Student;
                var selectedEvent = cmbEvent.SelectedItem as Eventss;

                if (selectedStudent == null || selectedEvent == null)
                {
                    MessageBox.Show("Пожалуйста, выберите студента и событие.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Создание нового объекта Participation_in_event
                var newParticipation = new Participation_in_event
                {
                    studentID = selectedStudent.studentID,
                    eventID = selectedEvent.eventID
                };

                var participationJson = JsonConvert.SerializeObject(newParticipation);
                var participationContent = new StringContent(participationJson, Encoding.UTF8, "application/json");

                var participationResponse = await client.PostAsync("Participation_in_event", participationContent);
                participationResponse.EnsureSuccessStatusCode();

                MessageBox.Show("Участие в событии добавлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadParticipation_Click(sender, e); // Перезагрузка данных после добавления
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private Participation_in_event selectedParticipation;

        private void DataGridParticipation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgParticipation.SelectedItem is Participation_in_event participation)
            {
                selectedParticipation = participation;

                // Устанавливаем выбранное значение для студента
                cmbStudent.SelectedItem = participation.Student;

                // Устанавливаем выбранное значение для события
                cmbEvent.SelectedItem = participation.Eventss;
            }
        }

        private async void btnUpdateParticipation_Click(object sender, RoutedEventArgs e)
        {
            if (selectedParticipation == null)
            {
                MessageBox.Show("Пожалуйста, выберите участие в мероприятии для обновления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var selectedStudent = cmbStudent.SelectedItem as Student;
                var selectedEvent = cmbEvent.SelectedItem as Eventss;

                if (selectedStudent == null || selectedEvent == null)
                {
                    MessageBox.Show("Пожалуйста, выберите студента и событие.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Обновляем идентификаторы студента и события в выбранном участии
                selectedParticipation.studentID = selectedStudent.studentID;
                selectedParticipation.eventID = selectedEvent.eventID;

                // Сериализуем обновленный объект участия
                var json = JsonConvert.SerializeObject(selectedParticipation);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Отправляем PUT запрос с использованием идентификаторов студента и события
                var response = await client.PutAsync($"Participation_in_event/{selectedParticipation.studentID}", content);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Участие в мероприятии обновлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearParticipationFields();
                LoadParticipation_Click(sender, e); // Перезагрузка данных после обновления
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnDeleteParticipation_Click(object sender, RoutedEventArgs e)
        {
            if (selectedParticipation == null)
            {
                MessageBox.Show("Пожалуйста, выберите участие в мероприятии для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var response = await client.DeleteAsync($"Participation_in_event/{selectedParticipation.studentID}");
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Участие в мероприятии удалено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearParticipationFields();
                LoadParticipation_Click(sender, e); // Перезагрузка данных после удаления
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearParticipationFields()
        {
            selectedParticipation = null;
            cmbStudent.SelectedItem = null;
            cmbEvent.SelectedItem = null;
        }





        private void LoadStudents(List<Student> students)
        {
            cmbStudent.ItemsSource = students;
            cmbStudent.DisplayMemberPath = "last_name";
            cmbStudent.SelectedValuePath = "studentID";
        }

        private void LoadEvents(List<Eventss> events)
        {
            cmbEvent.ItemsSource = events;
            cmbEvent.DisplayMemberPath = "name_of_event";
            cmbEvent.SelectedValuePath = "eventID";
        }







        private void ClearEventFields()
        {
            cmbEvent.Text = string.Empty;
            cmbEvent.Text = string.Empty;
        }


        private void parSort(object sender, RoutedEventArgs e)
        {
            string sortBy = ((ComboBoxItem)cmbSortBypar.SelectedItem).Content.ToString();
            ICollectionView view = CollectionViewSource.GetDefaultView(dgParticipation.ItemsSource);

            if (sortBy == "Ascending")
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("StudentName", ListSortDirection.Ascending));
            }
            else if (sortBy == "Descending")
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("StudentName", ListSortDirection.Descending));
            }
            else if (sortBy == "No Sorting")
            {
                view.SortDescriptions.Clear();
            }
        }

        private void parFilt(object sender, RoutedEventArgs e)
        {
            string filterText = parfilt.Text.Trim().ToLower();
            ICollectionView view = CollectionViewSource.GetDefaultView(dgParticipation.ItemsSource);

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                view.Filter = item =>
                {
                    var participationEvent = item as Participation_in_event;
                    return participationEvent.Eventss.name_of_event?.ToLower().Contains(filterText) == true;
                };
            }
            else
            {
                view.Filter = null;
            }
        }


        private void parSearch(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearchedpar.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                dgParticipation.UnselectAll();
                return;
            }

            dgParticipation.UpdateLayout();

            foreach (var item in dgParticipation.Items)
            {
                dgParticipation.ScrollIntoView(item);
                DataGridRow row = (DataGridRow)dgParticipation.ItemContainerGenerator.ContainerFromItem(item);

                if (row != null)
                {
                    if (item is Participation_in_event participationEvent)
                    {
                        var participation = participationEvent.Eventss; // Assuming Event is the property of Participation_in_event that is of type Eventss

                        if (participation != null && participation.name_of_event.ToLower().Contains(searchText))
                        {
                            row.Background = Brushes.Yellow; // Highlight row with matching value
                        }
                        else
                        {
                            row.Background = Brushes.White; // Return original color for other rows
                        }
                    }
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////
        private async void LoadAllStudents()
        {
            try
            {
                // Загрузите список всех студентов
                var response = await client.GetStringAsync("Student");
                var students = JsonConvert.DeserializeObject<List<Student>>(response);

                // Очистите ComboBox перед добавлением новых элементов
                cmbLastName.Items.Clear();

                // Добавьте last_name каждого студента в ComboBox
                foreach (var student in students)
                {
                    cmbLastName.Items.Add(student.last_name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки студентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void LoadProm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = await client.GetStringAsync("Promotion");
                var studentResponse = await client.GetStringAsync("Student");

                var promotions = JsonConvert.DeserializeObject<List<PROMPUN>>(response);
                var students = JsonConvert.DeserializeObject<List<Student>>(studentResponse);

                // Подгружаем все студентов в словарь для удобного доступа по ID
                var studentDictionary = students.ToDictionary(student => student.studentID);

                var promotionData = new List<PROMPUN>();
                LoadAllStudents();
                foreach (var promotion in promotions)
                {
                    // Находим студента для каждого объекта PROMPUN и присваиваем его свойству Student
                    if (studentDictionary.ContainsKey(promotion.studentID))
                    {
                        promotion.Student = studentDictionary[promotion.studentID];
                        promotionData.Add(promotion);
                    }
                }

                dgProm.ItemsSource = promotionData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void AddProm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var studentResponse = await client.GetStringAsync("Student");
                var students = JsonConvert.DeserializeObject<List<Student>>(studentResponse);
                string promotionName = txtPromName.Text.Trim();
                string punishmentName = txtPunishmentName.Text.Trim();

                // Получаем выбранный last_name из комбобокса
                string selectedLastName = cmbLastName.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(selectedLastName))
                {
                    MessageBox.Show("Выберите студента", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Находим studentID для выбранного last_name
                var student = students.FirstOrDefault(s => s.last_name == selectedLastName);
                if (student == null)
                {
                    MessageBox.Show("Студент с такой фамилией не найден", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                int studentID = student.studentID;

                var newPromotion = new PROMPUN
                {
                    name_of_promotion = promotionName,
                    name_of_punishment = punishmentName,
                    studentID = studentID
                };

                var promotionJson = JsonConvert.SerializeObject(newPromotion);
                var promotionContent = new StringContent(promotionJson, Encoding.UTF8, "application/json");

                var promotionResponse = await client.PostAsync("Promotion", promotionContent);
                promotionResponse.EnsureSuccessStatusCode();

                MessageBox.Show("Поощрение добавлено", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dataGridPromotions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Проверяем, выбрана ли какая-либо строка
            if (dgProm.SelectedItem is PROMPUN promotion)
            {
                selectedPromotion = promotion;
                txtPromName.Text = promotion.name_of_promotion;
                txtPunishmentName.Text = promotion.name_of_punishment;
                txtStudentID.Text = promotion.studentID.ToString();

                // Обновляем выбор в ComboBox для отображения текущего студента
                var student = promotion.Student;
                if (student != null)
                {
                    cmbLastName.SelectedItem = student.last_name;
                }
            }
        }

        private PROMPUN selectedPromotion;
        private async void DellProm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, выбрана ли запись в таблице
                if (selectedPromotion != null)
                {
                    // Получаем ID записи для удаления
                    int promotionIdToDelete = selectedPromotion.promotionID;

                    string requestUri = $"Promotion/{promotionIdToDelete}";

                    var response = await client.DeleteAsync(requestUri);
                    response.EnsureSuccessStatusCode();

                    MessageBox.Show("Поощрение удалено", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Выберите запись для удаления", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void UpdateProm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, выбрана ли запись в таблице
                if (selectedPromotion != null)
                {
                    // Получаем значения для обновления из полей ввода
                    string promotionName = txtPromName.Text.Trim();
                    string punishmentName = txtPunishmentName.Text.Trim();
                    int studentID;
                    if (!int.TryParse(txtStudentID.Text.Trim(), out studentID))
                    {
                        MessageBox.Show("Неверный ID студента", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Обновляем выбранную запись
                    selectedPromotion.name_of_promotion = promotionName;
                    selectedPromotion.name_of_punishment = punishmentName;
                    selectedPromotion.studentID = studentID;


                    var promotionJson = JsonConvert.SerializeObject(selectedPromotion);
                    var promotionContent = new StringContent(promotionJson, Encoding.UTF8, "application/json");

                    string requestUri = $"Promotion/{selectedPromotion.promotionID}";

                    var response = await client.PutAsync(requestUri, promotionContent);
                    response.EnsureSuccessStatusCode();

                    MessageBox.Show("Поощрение обновлено", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Выберите запись для обновления", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void poSort(object sender, RoutedEventArgs e)
        {
            string sortBy = ((ComboBoxItem)cmbSortBypo.SelectedItem).Content.ToString();
            ICollectionView view = CollectionViewSource.GetDefaultView(dgProm.ItemsSource);

            if (sortBy == "Ascending")
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("name_of_promotion", ListSortDirection.Ascending));
            }
            else if (sortBy == "Descending")
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("name_of_promotion", ListSortDirection.Descending));
            }
            else if (sortBy == "No Sorting")
            {
                view.SortDescriptions.Clear();
            }
        }

        private void poFilt(object sender, RoutedEventArgs e)
        {
            string filterText = pofilt.Text.Trim().ToLower();
            ICollectionView view = CollectionViewSource.GetDefaultView(dgProm.ItemsSource);

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                view.Filter = item => ((PROMPUN)item).name_of_promotion.ToLower().Contains(filterText);
            }
            else
            {
                view.Filter = null;
            }
        }

        private void poSearch(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearchedpo.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (var item in dgProm.Items)
                {
                    DataGridRow row = (DataGridRow)dgProm.ItemContainerGenerator.ContainerFromItem(item);
                    if (row != null)
                    {
                        row.Background = Brushes.White; // Return original color for all rows
                    }
                }
                return;
            }

            foreach (var item in dgProm.Items)
            {
                DataGridRow row = (DataGridRow)dgProm.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                {
                    if (item is PROMPUN promotion)
                    {
                        if (promotion.name_of_promotion.ToLower().Contains(searchText))
                        {
                            row.Background = Brushes.Yellow; // Highlight row with matching value
                        }
                        else
                        {
                            row.Background = Brushes.White; // Return original color for other rows
                        }
                    }
                }
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /*private async void LoadPun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = await client.GetStringAsync("Punishment");
                var studentResponse = await client.GetStringAsync("Student");

                var punishments = JsonConvert.DeserializeObject<List<Punishment>>(response);
                var students = JsonConvert.DeserializeObject<List<Student>>(studentResponse);

                var punishmentData = new List<dynamic>();

                foreach (var punishment in punishments)
                {
                    var student = students.FirstOrDefault(s => s.studentID == punishment.punishmentID);
                    if (student != null)
                    {
                        punishmentData.Add(new
                        {
                            StudentLastName = student.First_Name,
                            name_of_punishment = punishment.name_of_punishment
                        });
                    }
                }

                dgPun.ItemsSource = punishmentData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void AddPun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем данные для добавления
                string studentLastName = txtStudentFamilipu.Text.Trim();
                string punishmentName = txtPunName.Text.Trim();

                // Создаем новый объект Punishment
                var newPunishment = new Punishment
                {
                    punishmentID = int.Parse(studentLastName), // Предполагается, что это ID студента
                    name_of_punishment = punishmentName
                };

                // Сериализуем объект Punishment в JSON
                var punishmentJson = JsonConvert.SerializeObject(newPunishment);
                var punishmentContent = new StringContent(punishmentJson, Encoding.UTF8, "application/json");

                // Отправляем POST-запрос для добавления нового наказания
                var punishmentResponse = await client.PostAsync("Punishment", punishmentContent);
                punishmentResponse.EnsureSuccessStatusCode();

                // Показываем сообщение об успешном добавлении
                MessageBox.Show("Наказание добавлено", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void DellPun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем ID записи для удаления
                int punishmentIdToDelete;
                if (!int.TryParse(txtStudentFamilipu.Text, out punishmentIdToDelete))
                {
                    MessageBox.Show("Неверное ID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Формируем адрес запроса для удаления записи по ID
                string requestUri = $"Punishment/{punishmentIdToDelete}";

                // Отправляем DELETE-запрос для удаления записи
                var response = await client.DeleteAsync(requestUri);
                response.EnsureSuccessStatusCode();

                // Показываем сообщение об успешном удалении
                MessageBox.Show("Наказание удалено", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void UpdatePun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем данные для обновления
                int punishmentIdToUpdate;
                if (!int.TryParse(txtStudentFamilipu.Text, out punishmentIdToUpdate))
                {
                    MessageBox.Show("Неверный ID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string punishmentName = txtPunName.Text.Trim();
                string studentLastName = txtStudentFamilipu.Text.Trim();

                // Создаем объект Punishment с обновленными данными
                var updatedPunishment = new Punishment
                {
                    punishmentID = punishmentIdToUpdate,
                    name_of_punishment = punishmentName
                };

                // Сериализуем объект Punishment в JSON
                var punishmentJson = JsonConvert.SerializeObject(updatedPunishment);
                var punishmentContent = new StringContent(punishmentJson, Encoding.UTF8, "application/json");

                // Формируем адрес запроса для обновления записи по ID
                string requestUri = $"Punishment/{punishmentIdToUpdate}";

                // Отправляем PUT-запрос для обновления записи
                var response = await client.PutAsync(requestUri, punishmentContent);
                response.EnsureSuccessStatusCode();

                // Показываем сообщение об успешном обновлении
                MessageBox.Show("Наказание обновлено", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void punSort(object sender, RoutedEventArgs e)
        {
            string sortBy = ((ComboBoxItem)cmbSortBypun.SelectedItem).Content.ToString();
            ICollectionView view = CollectionViewSource.GetDefaultView(dgPun.ItemsSource);

            if (sortBy == "Ascending")
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("name_of_punishment", ListSortDirection.Ascending));
            }
            else if (sortBy == "Descending")
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("name_of_punishment", ListSortDirection.Descending));
            }
            else if (sortBy == "No Sorting")
            {
                view.SortDescriptions.Clear();
            }
        }
        private void punFilt(object sender, RoutedEventArgs e)
        {
            string filterText = punfilt.Text.Trim().ToLower();
            ICollectionView view = CollectionViewSource.GetDefaultView(dgPun.ItemsSource);

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                view.Filter = item =>
                {
                    var itemType = item.GetType();
                    var nameProperty = itemType.GetProperty("name_of_punishment");
                    if (nameProperty != null)
                    {
                        var nameValue = nameProperty.GetValue(item) as string;
                        return nameValue != null && nameValue.ToLower().Contains(filterText);
                    }
                    return false;
                };
            }
            else
            {
                view.Filter = null;
            }

            view.Refresh(); // Обновляем представление для применения фильтра
        }

        private void punSearch(object sender, RoutedEventArgs e)
        {
            string searchTextt = txtSearchpun.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchTextt))
            {
                dgProm.UnselectAll();
                return;
            }

            dgProm.UpdateLayout();

            foreach (var item in dgPun.Items)
            {
                dgPun.ScrollIntoView(item);
                DataGridRow row = (DataGridRow)dgPun.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                {
                    if (item is Punishment punishment)
                    {
                        if (punishment.name_of_punishment.ToLower().Contains(searchTextt))
                        {
                            row.Background = Brushes.Yellow; // Highlight row with matching value
                        }
                        else
                        {
                            row.Background = Brushes.White; // Return original color for other rows
                        }
                    }
                }
            }
        }*/
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///

        private async void btnLoadCreativity_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load data from API
                var creativityResponse = await client.GetStringAsync("creativity_activity");


                // Deserialize data
                var activities = JsonConvert.DeserializeObject<List<creativity_activity>>(creativityResponse);


                // Establish relationship between creativity activities and students
                // Assuming you want to display last names

                // Prepare combined data for DataGrid display

                // Display data in DataGrid
                // Assuming combinedData is a List<creativity_activity> populated correctly
                dgCreativity.ItemsSource = activities;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async void btnAddCreativityActivity_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем выбранного студента из комбобокса


                // Получаем данные из других контролов (например, название креативности)
                var creativityName = txtCreativityName.Text;
                if (string.IsNullOrWhiteSpace(creativityName))
                {
                    MessageBox.Show("Введите название креативности.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Создаем новый объект креативности
                var newCreativityActivity = new creativity_activity
                {
                    name_of_creativity = creativityName,

                    // Вы можете добавить другие свойства, если они есть
                };

                var json = JsonConvert.SerializeObject(newCreativityActivity);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("creativity_activity", content);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Креативность успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                btnLoadCreativity_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async void btnUpdateCreativityActivity_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем выбранную креативность из DataGrid
                var selectedCreativity = dgCreativity.SelectedItem as creativity_activity;
                if (selectedCreativity == null)
                {
                    MessageBox.Show("Выберите креативность для обновления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Получаем выбранного студента из комбобокса


                // Обновляем свойства выбранной креативности

                selectedCreativity.name_of_creativity = txtCreativityName.Text;

                var json = JsonConvert.SerializeObject(selectedCreativity);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"creativity_activity/{selectedCreativity.creativityID}", content);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Креативность успешно обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                btnLoadCreativity_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void btnDeleteCreativityActivity_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем выбранную креативность из DataGrid
                var selectedCreativity = dgCreativity.SelectedItem as creativity_activity;
                if (selectedCreativity == null)
                {
                    MessageBox.Show("Выберите креативность для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var response = await client.DeleteAsync($"creativity_activity/{selectedCreativity.creativityID}");
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Креативность успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                btnLoadCreativity_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnApplySortingCreativity_Click(object sender, RoutedEventArgs e)
        {
            var sortBy = cmbSortByCreativity.SelectedValue.ToString();
            var activities = dgCreativity.ItemsSource as List<creativity_activity>;

            if (sortBy == "Ascending")
            {
                dgCreativity.ItemsSource = activities.OrderBy(c => c.name_of_creativity).ToList();
            }
            else if (sortBy == "Descending")
            {
                dgCreativity.ItemsSource = activities.OrderByDescending(c => c.name_of_creativity).ToList();
            }
            else
            {
                btnLoadCreativity_Click(sender, e);
            }
        }

        private void btnSearchCreativity_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = txtSearchCreativity.Text.ToLower();
            var searchedList = (dgCreativity.ItemsSource as List<creativity_activity>).Where(c => c.name_of_creativity.ToLower().Contains(searchTerm)).ToList();
            dgCreativity.ItemsSource = searchedList;
        }

        private void btnCancelSearchCreativity_Click(object sender, RoutedEventArgs e)
        {
            btnLoadCreativity_Click(sender, e);
        }

        private void dgCreativity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedCreativity = dgCreativity.SelectedItem as creativity_activity;
            if (selectedCreativity != null)
            {
                txtCreativityID.Text = selectedCreativity.creativityID.ToString();
                txtTypeOfCreativity.Text = selectedCreativity.type_of_creativity.ToString();
                txtCreativityName.Text = selectedCreativity.name_of_creativity;

            }
        }
        private void btnSearch1Creativity_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearch1Creativity.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (var item in dgCreativity.Items)
                {
                    DataGridRow row = (DataGridRow)dgCreativity.ItemContainerGenerator.ContainerFromItem(item);
                    if (row != null)
                    {
                        row.Background = Brushes.White;
                    }
                }
                return;
            }

            foreach (var item in dgCreativity.Items)
            {
                DataGridRow row = (DataGridRow)dgCreativity.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                {
                    if (item is creativity_activity activity)
                    {
                        if (activity.name_of_creativity.ToLower().Contains(searchText))
                        {
                            row.Background = Brushes.Yellow;
                        }
                        else
                        {
                            row.Background = Brushes.White;
                        }
                    }
                }
            }
        }

        private void btnCancelSearch1Creativity_Click(object sender, RoutedEventArgs e)
        {
            txtSearch1Creativity.Clear();
            foreach (var item in dgCreativity.Items)
            {
                DataGridRow row = (DataGridRow)dgCreativity.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                {
                    row.Background = Brushes.White;
                }
            }
        }
        /// <summary>
        /// //////////////////////////////////////
        /// </summary>
        /// <returns></returns>
        private async Task<List<Student>> GetStudentsAsync()
        {
            var response = await client.GetStringAsync("Student");
            return JsonConvert.DeserializeObject<List<Student>>(response);
        }

        private async Task<List<creativity_activity>> GetCreativityActivitiesAsync()
        {
            var response = await client.GetStringAsync("creativity_activity");
            return JsonConvert.DeserializeObject<List<creativity_activity>>(response);
        }

        private async Task<List<Student_Creativity>> GetStudentCreativityAsync()
        {
            var response = await client.GetStringAsync("Student_Creativity");
            return JsonConvert.DeserializeObject<List<Student_Creativity>>(response);
        }
        private async Task AddStudentCreativityAsync(Student_Creativity newStudentCreativity)
        {
            var json = JsonConvert.SerializeObject(newStudentCreativity);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("Student_Creativity", content);
            response.EnsureSuccessStatusCode();
        }

        private async Task UpdateStudentCreativityAsync(Student_Creativity selectedStudentCreativity)
        {
            var json = JsonConvert.SerializeObject(selectedStudentCreativity);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"Student_Creativity/{selectedStudentCreativity.student_creativity}", content);
            response.EnsureSuccessStatusCode();
        }

        private async Task DeleteStudentCreativityAsync(Student_Creativity selectedStudentCreativity)
        {
            var response = await client.DeleteAsync($"Student_Creativity/{selectedStudentCreativity.student_creativity}");
            response.EnsureSuccessStatusCode();
        }


        private async Task LoadStudentCreativityDataAsync()
        {
            try
            {
                var studentResponse = await client.GetStringAsync("Student");
                var creativityResponse = await client.GetStringAsync("Creativity_Activity");

                var students = JsonConvert.DeserializeObject<List<Student>>(studentResponse);
                var creativityActivities = JsonConvert.DeserializeObject<List<creativity_activity>>(creativityResponse);
                cmbStudents.ItemsSource = students;
                cmbStudents.DisplayMemberPath = "last_name";
                cmbStudents.SelectedValuePath = "studentID";

                cmbCreativity.ItemsSource = creativityActivities;
                cmbCreativity.DisplayMemberPath = "name_of_creativity";
                cmbCreativity.SelectedValuePath = "creativityID";


                var studentCreativityList = await GetStudentCreativityAsync();

                foreach (var sc in studentCreativityList)
                {
                    // Загружаем связанные данные для студента
                    sc.Student = students.FirstOrDefault(s => s.studentID == sc.studentID);
                    // Загружаем связанные данные для активности творчества
                    sc.creativity_activity = creativityActivities.FirstOrDefault(c => c.creativityID == sc.creativityID);
                }


                dgStudentCreativity.ItemsSource = studentCreativityList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading student creativity data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void ClearStudentCreativityFields()
        {
            //txtStudentCreativityId.Clear();//
            cmbCreativity.SelectedIndex = -1;
            cmbStudents.SelectedIndex = -1;
        }

        private async void btnLoadStudentCreativity_Click(object sender, RoutedEventArgs e)
        {
            await LoadStudentCreativityDataAsync();
        }

        private async void btnAddStudentCreativity_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, что пользователь выбрал студента и креативность
                if (cmbStudents.SelectedValue == null || cmbCreativity.SelectedValue == null)
                {
                    MessageBox.Show("Пожалуйста, выберите студента и активность творчества.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверяем выбранные значения
                var selectedStudentID = (int)cmbStudents.SelectedValue;
                var selectedCreativityID = (int)cmbCreativity.SelectedValue;

                // Отладочное сообщение
                Console.WriteLine($"Selected Student ID: {selectedStudentID}");
                Console.WriteLine($"Selected Creativity ID: {selectedCreativityID}");

                // Создаем новый объект Student_Creativity и инициализируем его поля
                var newStudentCreativity = new Student_Creativity
                {
                    studentID = selectedStudentID,
                    creativityID = selectedCreativityID
                };

                // Сериализуем объект в JSON
                var json = JsonConvert.SerializeObject(newStudentCreativity);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Логируем сериализованные данные
                Console.WriteLine($"JSON Data: {json}");

                // Отправляем POST запрос для добавления нового объекта
                var response = await client.PostAsync("Student_Creativity", content);

                // Если ответ содержит ошибку, логируем содержимое ответа
                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Status Code: {response.StatusCode}");
                    Console.WriteLine($"Response Content: {responseContent}");
                    MessageBox.Show($"Ошибка на сервере: {responseContent}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Показываем сообщение об успешном добавлении
                MessageBox.Show("Участие в творческой деятельности добавлено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Перезагружаем данные и очищаем поля
                await LoadStudentCreativityDataAsync();
                ClearStudentCreativityFields();
            }
            catch (Exception ex)
            {
                // Показываем сообщение об ошибке
                MessageBox.Show($"Ошибка при добавлении участия в творческой деятельности: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async void btnUpdateStudentCreativity_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgStudentCreativity.SelectedItem is Student_Creativity selectedStudentCreativity)
                {
                    if (cmbStudents.SelectedValue == null || cmbCreativity.SelectedValue == null)
                    {
                        throw new Exception("Please select both student and creativity.");
                    }

                    selectedStudentCreativity.studentID = (int)cmbStudents.SelectedValue;
                    selectedStudentCreativity.creativityID = (int)cmbCreativity.SelectedValue;

                    await UpdateStudentCreativityAsync(selectedStudentCreativity);

                    MessageBox.Show("Student creativity updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadStudentCreativityDataAsync();
                    ClearStudentCreativityFields();
                }
                else
                {
                    MessageBox.Show("Please select a student creativity entry to update.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating student creativity: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnDeleteStudentCreativity_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgStudentCreativity.SelectedItem is Student_Creativity selectedStudentCreativity)
                {
                    await DeleteStudentCreativityAsync(selectedStudentCreativity);

                    MessageBox.Show("Student creativity deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadStudentCreativityDataAsync();
                    ClearStudentCreativityFields();
                }
                else
                {
                    MessageBox.Show("Please select a student creativity entry to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting student creativity: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void dgStudentCreativity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgStudentCreativity.SelectedItem is Student_Creativity selectedStudentCreativity)
            {
                //txtStudentCreativityId.Text = selectedStudentCreativity.student_creativity.ToString();//
                cmbStudents.SelectedValue = selectedStudentCreativity.studentID;
                cmbCreativity.SelectedValue = selectedStudentCreativity.creativityID;
            }
        }
        private void parSort1(object sender, RoutedEventArgs e)
        {
            var sortDirection = ((ComboBoxItem)cmbSort1.SelectedItem)?.Content.ToString();

            // Clear existing sort descriptions
            dgStudentCreativity.Items.SortDescriptions.Clear();

            if (sortDirection == "Ascending")
            {
                dgStudentCreativity.Items.SortDescriptions.Add(new SortDescription("Student.last_name", ListSortDirection.Ascending));
            }
            else if (sortDirection == "Descending")
            {
                dgStudentCreativity.Items.SortDescriptions.Add(new SortDescription("Student.last_name", ListSortDirection.Descending));
            }
        }

        private void parSearch1(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearch1.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (var item in dgStudentCreativity.Items)
                {
                    DataGridRow row = (DataGridRow)dgStudentCreativity.ItemContainerGenerator.ContainerFromItem(item);
                    if (row != null)
                    {
                        row.Background = Brushes.White;
                    }
                }
                return;
            }

            foreach (var item in dgStudentCreativity.Items)
            {
                DataGridRow row = (DataGridRow)dgStudentCreativity.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                {
                    if (item is Student_Creativity studentCreativity)
                    {
                        if (studentCreativity.Student.last_name.ToLower().Contains(searchText) ||
                            studentCreativity.creativity_activity.name_of_creativity.ToLower().Contains(searchText))
                        {
                            row.Background = Brushes.Yellow;
                        }
                        else
                        {
                            row.Background = Brushes.White;
                        }
                    }
                }
            }
        }


        private void parFilt1(object sender, RoutedEventArgs e)
        {
            string filterTerm = txtFilterCreativityName.Text.ToLower();

            // Filter the data grid based on the filter term
            ICollectionView view = CollectionViewSource.GetDefaultView(dgStudentCreativity.ItemsSource);
            view.Filter = obj =>
            {
                var studentCreativity = obj as Student_Creativity;
                return studentCreativity.creativity_activity.name_of_creativity.ToLower().Contains(filterTerm);
            };
        }




        private void ClearFamilyFields()
        {

            txtFamilyCompleteness.Clear();
        }

        private async void LoadButton(object sender, RoutedEventArgs e)
        {
            try
            {
                var gradeResponse = await client.GetStringAsync("grade");
                var studentResponse = await client.GetStringAsync("student");

                var gradeList = JsonConvert.DeserializeObject<List<grade>>(gradeResponse);
                var students = JsonConvert.DeserializeObject<List<Student>>(studentResponse);

                // Объединяем данные студентов и оценок, подставляем полное имя студента в колонку StudentID
                var gradeWithStudentNameList = gradeList.Join(
                    students,
                    grade => grade.studentID,
                    student => student.studentID,
                    (grade, student) => new
                    {
                        StudentFullName = $"{student.last_name} {student.First_Name}",
                        grade.Maths_grades,
                        grade.Russian_language,
                        grade.litra,
                        grade.fizra,
                        grade.it,
                        grade.izo,
                        grade.technologia,
                        grade.obsh,
                        grade.english_lang,
                        grade.history
                    }).Select(x => new grade
                    {
                        StudentFullName = x.StudentFullName,
                        Maths_grades = x.Maths_grades,
                        Russian_language = x.Russian_language,
                        litra = x.litra,
                        fizra = x.fizra,
                        it = x.it,
                        izo = x.izo,
                        technologia = x.technologia,
                        obsh = x.obsh,
                        english_lang = x.english_lang,
                        history = x.history
                    }).ToList();

                // Устанавливаем источник данных для DataGrid
                GradesDataGrid.ItemsSource = gradeWithStudentNameList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        private DataGridCell selectedCell;

        



        private void GradesDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Находим ячейку, на которую кликнули
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep is DataGridCell cell && !cell.IsEditing)
            {
                cell.IsEditing = true;

                // Сохраняем выбранную ячейку
                selectedCell = cell;
            }
        }

        private void GradesDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (selectedCell != null && e.Key == Key.Enter)
            {
                // Завершаем редактирование ячейки по нажатию Enter
                selectedCell.IsEditing = false;
                selectedCell = null;
            }
        }



        private async void UpdateGradeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = GradesDataGrid.SelectedValue as grade;
                int id = x.studentID;
               
                if (GradesDataGrid.SelectedItem is grade selgrade)
                {
                    selgrade.studentID = id;
                    selgrade.Maths_grades = MathsTextBox.Text;
                    selgrade.Russian_language = RussianTextBox.Text;
                    selgrade.litra = LiteratureTextBox.Text;
                    selgrade.fizra = PETextBox.Text;
                    selgrade.it = ITTextBox.Text;
                    selgrade.izo = ArtTextBox.Text;
                    selgrade.technologia = TechnologyTextBox.Text;
                    selgrade.obsh = SocialStudiesTextBox.Text;
                    selgrade.english_lang = EnglishTextBox.Text;
                    selgrade.history = HistoryTextBox.Text;

                    var json = JsonConvert.SerializeObject(selgrade);
                    MessageBox.Show(json);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    var response = await client.PutAsync($"grade/{selgrade.studentID}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Оценки успешно обновлены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadButton(sender, e); 
                    }else{
                        MessageBox.Show($"Произошла ошибка при обновлении оценок. Код ошибки: {response.StatusCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите студента для обновления оценок.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void GradesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GradesDataGrid.SelectedItem is grade selectedgrades)
            {
               
                MathsTextBox.Text = selectedgrades.Maths_grades;
                RussianTextBox.Text = selectedgrades.Russian_language;
                LiteratureTextBox.Text = selectedgrades.litra;
                PETextBox.Text = selectedgrades.fizra;
                ITTextBox.Text = selectedgrades.it;
                ArtTextBox.Text = selectedgrades.izo;
                TechnologyTextBox.Text = selectedgrades.technologia;
                SocialStudiesTextBox.Text = selectedgrades.obsh;
                EnglishTextBox.Text = selectedgrades.english_lang;
                HistoryTextBox.Text = selectedgrades.history;
            }
        }

        private void CalculateAverageButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedGrade = GradesDataGrid.SelectedItem as grade; // Замените на ваш класс студента

            if (selectedGrade != null)
            {
                // Обновляем отображение оценок
                selectedGrade.Maths_grades = FormatGrades(selectedGrade.Maths_grades);
                selectedGrade.Russian_language = FormatGrades(selectedGrade.Russian_language);
                selectedGrade.litra = FormatGrades(selectedGrade.litra);
                selectedGrade.fizra = FormatGrades(selectedGrade.fizra);
                selectedGrade.it = FormatGrades(selectedGrade.it);
                selectedGrade.izo = FormatGrades(selectedGrade.izo);
                selectedGrade.technologia = FormatGrades(selectedGrade.technologia);
                selectedGrade.obsh = FormatGrades(selectedGrade.obsh);
                selectedGrade.english_lang = FormatGrades(selectedGrade.english_lang);
                selectedGrade.history = FormatGrades(selectedGrade.history);

                // Обновляем DataGrid для отображения изменений
                GradesDataGrid.Items.Refresh();

                // Собираем оценки из объекта студента (замените на ваши свойства)
                Dictionary<string, string> subjectGrades = new Dictionary<string, string>
        {
            { "Maths", selectedGrade.Maths_grades },
            { "Russian", selectedGrade.Russian_language },
            { "Literature", selectedGrade.litra },
            { "PE", selectedGrade.fizra },
            { "IT", selectedGrade.it },
            { "Art", selectedGrade.izo },
            { "Technology", selectedGrade.technologia },
            { "Social Studies", selectedGrade.obsh },
            { "English", selectedGrade.english_lang },
            { "History", selectedGrade.history }
        };

                // Выводим средние баллы
                StringBuilder resultMessage = new StringBuilder();
                resultMessage.AppendLine($"Средние баллы студента {selectedGrade.StudentFullName}:");

                foreach (var subject in subjectGrades)
                {
                    var grades = ParseGrades(subject.Value);
                    if (grades.Count > 0)
                    {
                        double averageGrade = grades.Average();
                        resultMessage.AppendLine($"{subject.Key}: {averageGrade.ToString("0.##")}");
                    }
                    else
                    {
                        resultMessage.AppendLine($"{subject.Key}: Нет оценок");
                    }
                }

                // Выводим результат
                MessageBox.Show(resultMessage.ToString());
            }
            else
            {
                MessageBox.Show("Выберите студента из списка.");
            }
        }


        private string FormatGrades(string grades)
        {
            if (!string.IsNullOrWhiteSpace(grades) && grades.All(char.IsDigit))
            {
                return string.Join(" ", grades.Select(c => c.ToString()));
            }
            return grades;
        }

        

        private void GenerateChartButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedGrade = GradesDataGrid.SelectedItem;

            // Проверка, что студент выбран
            if (selectedGrade == null)
            {
                MessageBox.Show("Выберите студента из списка.");
                return;
            }

            // Получаем имя выбранного студента
            string studentName = selectedGrade.GetType().GetProperty("StudentFullName")?.GetValue(selectedGrade, null)?.ToString();

            // Словарь для хранения средних оценок по каждому предмету
            Dictionary<string, double> averageGrades = new Dictionary<string, double>
    {
        { "Maths", CalculateAverage("Maths_grades") },
        { "Russian", CalculateAverage("Russian_language") },
        { "Literature", CalculateAverage("litra") },
        { "PE", CalculateAverage("fizra") },
        { "IT", CalculateAverage("it") },
        { "Art", CalculateAverage("izo") },
        { "Technology", CalculateAverage("technologia") },
        { "Social Studies", CalculateAverage("obsh") },
        { "English", CalculateAverage("english_lang") },
        { "History", CalculateAverage("history") }
    };

            // Вывод для отладки
            foreach (var subject in averageGrades)
            {
                Console.WriteLine($"{subject.Key}: {subject.Value}");
            }

            // Создание окна для отображения графика
            var chartWindow = new Window
            {
                Title = $"Средние баллы по предметам для {studentName}",
                Width = 800,
                Height = 600
            };

            // Создание и настройка столбчатой диаграммы
            var cartesianChart = new CartesianChart
            {
                Series = new SeriesCollection
        {
            new ColumnSeries
            {
                Title = "Средние баллы",
                Values = new ChartValues<double>(averageGrades.Values)
            }
        },
                AxisX = new AxesCollection
        {
            new Axis
            {
                Title = "Предметы",
                Labels = averageGrades.Keys.ToArray()
            }
        },
                AxisY = new AxesCollection
        {
            new Axis
            {
                Title = "Средний балл",
                LabelFormatter = value => value.ToString("N2")
            }
        }
            };

            // Отображение графика в новом окне
            chartWindow.Content = cartesianChart;
            chartWindow.Show();
        }


        private double CalculateAverage(string subject)
        {
            // Получаем выбранного студента
            var selectedGrade = GradesDataGrid.SelectedItem;

            // Проверка, что студент выбран
            if (selectedGrade != null)
            {
                // Получаем строку оценок для данного предмета
                string gradesString = selectedGrade.GetType().GetProperty(subject)?.GetValue(selectedGrade, null)?.ToString();
                if (!string.IsNullOrEmpty(gradesString))
                {
                    // Разбираем строку на отдельные оценки
                    var grades = ParseGrades(gradesString);
                    if (grades.Count > 0)
                    {
                        // Проверка содержимого списка оценок
                        Console.WriteLine($"Subject: {subject}, Grades: {string.Join(", ", grades)}");

                        // Вычисляем среднее значение всех оценок
                        return grades.Average();
                    }
                }
            }

            // Если оценки не найдены, возвращаем 0.0
            return 0.0;
        }

        private List<double> ParseGrades(string gradesString)
        {
            List<double> grades = new List<double>();
            if (!string.IsNullOrEmpty(gradesString))
            {
                // Разделяем строку на части
                string[] gradeParts = gradesString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in gradeParts)
                {
                    // Преобразуем каждую часть в double
                    if (double.TryParse(part, out double grade))
                    {
                        grades.Add(grade);
                    }
                    else
                    {
                        // Отладочное сообщение в случае неудачи
                        Console.WriteLine($"Failed to parse grade part: {part}");
                    }
                }
            }
            return grades;
        }





    }
}

    

