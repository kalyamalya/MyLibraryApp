using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;
using System.Drawing;

namespace personalLibrary
{
    public partial class FormMain : Form
    {
        public FormMain(string userLogin)
        {
            InitializeComponent();
            labelWelcome.Text = $"Добро пожаловать, {userLogin}!";

            // Заполняем ComboBox значениями рейтинга от 1 до 5
            comboBoxRating.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
            comboBoxRating.SelectedIndex = -1; // По умолчанию ничего не выбрано
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonAddBook_Click(object sender, EventArgs e)
        {
            // Получаем данные из полей
            string title = textBoxTitle.Text;
            string author = textBoxAuthor.Text;
            string publisher = textBoxPublisher.Text;
            string genre = textBoxGenre.Text;
            string rating = comboBoxRating.SelectedItem?.ToString(); // Получаем выбранное значение

            // Проверяем, чтобы все обязательные поля были заполнены
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author) ||
                string.IsNullOrWhiteSpace(publisher) || string.IsNullOrWhiteSpace(genre) ||
                string.IsNullOrEmpty(rating))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            // Определяем доступность книги на основе выбранной радиокнопки
            int availability = 1; // По умолчанию доступна
            if (radioButton2.Checked) // Если выбрана радиокнопка "Недоступные"
            {
                availability = 0; // Книга недоступна
            }

            // Запрос на добавление книги в базу данных
            string query = "INSERT INTO books (title, author, publisher, genre, rating, availability) " +
                           "VALUES (@title, @author, @publisher, @genre, @rating, @availability)";

            DB db = new DB();
            MySqlCommand command = new MySqlCommand(query, db.getConnection());
            command.Parameters.Add("@title", MySqlDbType.VarChar).Value = title;
            command.Parameters.Add("@author", MySqlDbType.VarChar).Value = author;
            command.Parameters.Add("@publisher", MySqlDbType.VarChar).Value = publisher;
            command.Parameters.Add("@genre", MySqlDbType.VarChar).Value = genre;
            command.Parameters.Add("@rating", MySqlDbType.Int32).Value = Convert.ToInt32(rating);
            command.Parameters.Add("@availability", MySqlDbType.Int32).Value = availability;

            // Выполняем запрос
            db.openConnection();
            if (command.ExecuteNonQuery() == 1)
            {
                MessageBox.Show("Книга добавлена!");

                // Очищаем поля после успешного добавления
                textBoxTitle.Clear();
                textBoxAuthor.Clear();
                textBoxPublisher.Clear();
                textBoxGenre.Clear();
                comboBoxRating.SelectedIndex = -1;

                // Обновляем DataGridView
                LoadBooks();
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении книги.");
            }

            db.closeConnection();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            string title = textBoxTitle.Text;
            string author = textBoxAuthor.Text;
            string publisher = textBoxPublisher.Text;
            string genre = textBoxGenre.Text;
            string rating = comboBoxRating.SelectedItem?.ToString();

            // Стартовый запрос
            string query = "SELECT * FROM books WHERE 1=1";

            // Добавляем фильтры по введенным данным
            if (!string.IsNullOrEmpty(title))
                query += " AND title LIKE @title";
            if (!string.IsNullOrEmpty(author))
                query += " AND author LIKE @author";
            if (!string.IsNullOrEmpty(publisher))
                query += " AND publisher LIKE @publisher";
            if (!string.IsNullOrEmpty(genre))
                query += " AND genre LIKE @genre";
            if (!string.IsNullOrEmpty(rating))
                query += " AND rating = @rating";

            // Проверяем состояние CheckBox и RadioButton
            if (!checkBoxShowAll.Checked) // Если "Показать все" не выбран
            {
                if (radioButton1.Checked) // Доступные книги
                    query += " AND availability = 1";
                else if (radioButton2.Checked) // Недоступные книги
                    query += " AND availability = 0";
            }

            // Настраиваем подключение и команду
            DB db = new DB();
            MySqlCommand command = new MySqlCommand(query, db.getConnection());

            // Привязываем параметры для предотвращения SQL-инъекций
            if (!string.IsNullOrEmpty(title))
                command.Parameters.Add("@title", MySqlDbType.VarChar).Value = "%" + title + "%";
            if (!string.IsNullOrEmpty(author))
                command.Parameters.Add("@author", MySqlDbType.VarChar).Value = "%" + author + "%";
            if (!string.IsNullOrEmpty(publisher))
                command.Parameters.Add("@publisher", MySqlDbType.VarChar).Value = "%" + publisher + "%";
            if (!string.IsNullOrEmpty(genre))
                command.Parameters.Add("@genre", MySqlDbType.VarChar).Value = "%" + genre + "%";
            if (!string.IsNullOrEmpty(rating))
                command.Parameters.Add("@rating", MySqlDbType.Int32).Value = Convert.ToInt32(rating);

            // Выполняем запрос
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);

            // Отображаем результаты в DataGridView
            dataGridViewBooks.DataSource = table;
        }

        private void checkBoxShowAll_CheckedChanged(object sender, EventArgs e)
        {
            bool showAll = checkBoxShowAll.Checked;

            // Отключаем или включаем RadioButtons
            radioButton1.Enabled = !showAll;
            radioButton2.Enabled = !showAll;

            // Если "Показать все" включен, сбрасываем выбор радиокнопок
            if (showAll)
            {
                radioButton1.Checked = false;
                radioButton2.Checked = false;
            }
            else
            {
                // По умолчанию выбираем "Доступные", если радиокнопки становятся активными
                radioButton1.Checked = true;
            }
        }

        private void LoadBooks()
        {
            string query = "SELECT * FROM books";

            DB db = new DB();
            MySqlDataAdapter adapter = new MySqlDataAdapter(query, db.getConnection());
            DataTable table = new DataTable();
            adapter.Fill(table);

            dataGridViewBooks.DataSource = table;

            db.closeConnection();
        }

        private void buttonDeleteBook_Click(object sender, EventArgs e)
        {
            // Проверяем, выбрана ли хотя бы одна строка
            if (dataGridViewBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите книгу для удаления.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Получаем ID книги из выбранной строки
            int bookId = Convert.ToInt32(dataGridViewBooks.SelectedRows[0].Cells[0].Value);
            string bookTitle = dataGridViewBooks.SelectedRows[0].Cells[1].Value.ToString(); // Название книги для отображения в диалоге

            // Запрашиваем у пользователя подтверждение удаления
            DialogResult result = MessageBox.Show($"Вы уверены, что хотите удалить книгу \"{bookTitle}\"?",
                                                  "Подтверждение удаления",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question);

            // Если пользователь нажал "Да"
            if (result == DialogResult.Yes)
            {
                // Удаляем книгу из базы данных
                string query = "DELETE FROM books WHERE id = @id";

                DB db = new DB();
                MySqlCommand command = new MySqlCommand(query, db.getConnection());
                command.Parameters.Add("@id", MySqlDbType.Int32).Value = bookId;

                db.openConnection();
                if (command.ExecuteNonQuery() == 1)
                {
                    MessageBox.Show("Книга успешно удалена!");

                    // Обновляем DataGridView после удаления книги
                    if (checkBoxShowAll.Checked)
                    {
                        LoadBooks(); // Если выбрано "Показать все", обновляем все записи
                    }
                    else
                    {
                        buttonSearch_Click(sender, e); // Иначе обновляем данные на основе текущего фильтра
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка при удалении книги.");
                }

                db.closeConnection();
            }
        }

        Point lastPoint;
        private void panelMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }

        private void panelMain_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = new Point(e.X, e.Y);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = new Point(e.X, e.Y);
        }

        private void buttonEditBook_Click(object sender, EventArgs e)
        {
            // Проверяем, выбрана ли строка в DataGridView
            if (dataGridViewBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите книгу для редактирования.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Получаем данные выбранной книги
            int bookId = Convert.ToInt32(dataGridViewBooks.SelectedRows[0].Cells[0].Value);

            // Получаем данные из текстовых полей
            string title = textBoxTitle.Text;
            string author = textBoxAuthor.Text;
            string publisher = textBoxPublisher.Text;
            string genre = textBoxGenre.Text;
            string rating = comboBoxRating.SelectedItem?.ToString();
            int availability = radioButton1.Checked ? 1 : 0;

            // Проверяем, чтобы все поля были заполнены
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author) ||
                string.IsNullOrWhiteSpace(publisher) || string.IsNullOrWhiteSpace(genre) ||
                string.IsNullOrEmpty(rating))
            {
                MessageBox.Show("Пожалуйста, заполните все поля для редактирования.");
                return;
            }

            // Запрос на обновление книги
            string query = "UPDATE books SET title = @title, author = @author, " +
                           "publisher = @publisher, genre = @genre, rating = @rating, " +
                           "availability = @availability WHERE id = @id";

            DB db = new DB();
            MySqlCommand command = new MySqlCommand(query, db.getConnection());
            command.Parameters.Add("@title", MySqlDbType.VarChar).Value = title;
            command.Parameters.Add("@author", MySqlDbType.VarChar).Value = author;
            command.Parameters.Add("@publisher", MySqlDbType.VarChar).Value = publisher;
            command.Parameters.Add("@genre", MySqlDbType.VarChar).Value = genre;
            command.Parameters.Add("@rating", MySqlDbType.Int32).Value = Convert.ToInt32(rating);
            command.Parameters.Add("@availability", MySqlDbType.Int32).Value = availability;
            command.Parameters.Add("@id", MySqlDbType.Int32).Value = bookId;

            // Выполняем запрос
            db.openConnection();
            if (command.ExecuteNonQuery() == 1)
            {
                MessageBox.Show("Книга успешно обновлена!");

                // Очищаем поля после редактирования
                textBoxTitle.Clear();
                textBoxAuthor.Clear();
                textBoxPublisher.Clear();
                textBoxGenre.Clear();
                comboBoxRating.SelectedIndex = -1;
                radioButton1.Checked = false;
                radioButton2.Checked = false;

                // Обновляем DataGridView
                if (checkBoxShowAll.Checked)
                {
                    LoadBooks(); // Если выбрано "Показать все", обновляем все записи
                }
                else
                {
                    buttonSearch_Click(sender, e); // Иначе обновляем данные на основе текущего фильтра
                }
            }
            else
            {
                MessageBox.Show("Ошибка при обновлении книги.");
            }

            db.closeConnection();
        }
        private void dataGridViewBooks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Проверяем, что строка выбрана
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridViewBooks.Rows[e.RowIndex];

                // Загружаем данные в поля
                textBoxTitle.Text = row.Cells["title"].Value.ToString();
                textBoxAuthor.Text = row.Cells["author"].Value.ToString();
                textBoxPublisher.Text = row.Cells["publisher"].Value.ToString();
                textBoxGenre.Text = row.Cells["genre"].Value.ToString();
                comboBoxRating.SelectedItem = row.Cells["rating"].Value.ToString();
                if (Convert.ToInt32(row.Cells["availability"].Value) == 1)
                {
                    radioButton1.Checked = true; // Доступна
                }
                else
                {
                    radioButton2.Checked = true; // Недоступна
                }
            }
        }
    }
}