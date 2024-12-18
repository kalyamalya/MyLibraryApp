using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace personalLibrary
{
    public partial class FormRegister : Form
    {
        public FormRegister()
        {
            InitializeComponent();
            textBoxLogin.Text = "Введите логин";
            textBoxPassword.Text = "Введите пароль";
            textBoxName.Text = "Введите имя";
            textBoxMail.Text = "Введите e-mail";
            textBoxLogin.ForeColor = Color.Gray;
            textBoxPassword.ForeColor = Color.Gray;
            textBoxName.ForeColor = Color.Gray;
            textBoxMail.ForeColor = Color.Gray;
            textBoxPassword.UseSystemPasswordChar = false;
            this.textBoxPassword.Size = new Size(this.textBoxPassword.Width, 64);
            this.textBoxPassword.AutoSize = false;
        }

        Point lastPoint;
        private void panelRegister_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = new Point(e.X, e.Y);
        }

        private void panelRegister_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void textBoxName_Enter(object sender, EventArgs e)
        {
            if (textBoxName.Text == "Введите имя")
            {
                textBoxName.Text = "";
                textBoxName.ForeColor = Color.Black;
            }
        }


        private void textBoxName_Leave(object sender, EventArgs e)
        {
            if (textBoxName.Text == "")
            {
                textBoxName.Text = "Введите имя";
                textBoxName.ForeColor = Color.Gray;
            }
        }

        private void textBoxLogin_Enter(object sender, EventArgs e)
        {
            if (textBoxLogin.Text == "Введите логин")
            {
                textBoxLogin.Text = "";
                textBoxLogin.ForeColor = Color.Black;
            }
        }

        private void textBoxLogin_Leave(object sender, EventArgs e)
        {
            if (textBoxLogin.Text == "")
            {
                textBoxLogin.Text = "Введите логин";
                textBoxLogin.ForeColor = Color.Gray;
            }
        }

        private void textBoxPassword_Enter(object sender, EventArgs e)
        {
            if (textBoxPassword.Text == "Введите пароль")
            {
                textBoxPassword.Text = "";
                textBoxPassword.ForeColor = Color.Black;
                textBoxPassword.UseSystemPasswordChar = true;
            }
        }

        private void textBoxPassword_Leave(object sender, EventArgs e)
        {
            if (textBoxPassword.Text == "")
            {
                textBoxPassword.UseSystemPasswordChar = false;
                textBoxPassword.Text = "Введите пароль";
                textBoxPassword.ForeColor = Color.Gray;

            }
        }

        private void textBoxMail_Enter(object sender, EventArgs e)
        {
            if (textBoxMail.Text == "Введите e-mail")
            {
                textBoxMail.Text = "";
                textBoxMail.ForeColor = Color.Black;
            }
        }

        private void textBoxMail_Leave(object sender, EventArgs e)
        {
            if (textBoxMail.Text == "")
            {
                textBoxMail.Text = "Введите e-mail";
                textBoxMail.ForeColor = Color.Gray;
            }
        }

        private void buttonRegister_Click(object sender, EventArgs e)
        {
            if (textBoxName.Text == "Введите имя")
            {
                MessageBox.Show("Введите имя");
                return;
            }

            if (textBoxMail.Text == "Введите e-mail")
            {
                MessageBox.Show("Введите e-mail");
                return;
            }

            if (textBoxLogin.Text == "Введите логин")
            {
                MessageBox.Show("Введите логин");
                return;
            }

            if (textBoxPassword.Text == "Введите пароль")
            {
                MessageBox.Show("Введите пароль");
                return;
            }

            if (checkUser() || checkMail())
                return;

            DB db = new DB();
            MySqlCommand command = new MySqlCommand("INSERT INTO `users` (`login`, `pass`, `name`, `mail`) VALUES (@login, @pass, @name, @email)", db.getConnection());

            command.Parameters.Add("@login", MySqlDbType.VarChar).Value = textBoxLogin.Text;
            command.Parameters.Add("@pass", MySqlDbType.VarChar).Value = textBoxPassword.Text;
            command.Parameters.Add("@name", MySqlDbType.VarChar).Value = textBoxName.Text;
            command.Parameters.Add("@email", MySqlDbType.VarChar).Value = textBoxMail.Text;
            
            db.openConnection();

            if (command.ExecuteNonQuery() == 1)
                MessageBox.Show("Вы зарегистрировались!");
            else
                MessageBox.Show("Произошла ошибка при регистрации. Попробуйте снова");

            db.closeConnection();
        }

        public Boolean checkUser()
        {
            DB db = new DB();

            DataTable table = new DataTable();

            MySqlDataAdapter adapter = new MySqlDataAdapter();

            MySqlCommand command = new MySqlCommand("SELECT * FROM `users` WHERE `login` = @lU", db.getConnection());
            command.Parameters.Add("@lU", MySqlDbType.VarChar).Value = textBoxLogin.Text;

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count > 0)
            {
                MessageBox.Show("Такой логин уже занят");
                return true;
            }
            else
                return false;
        }

        public Boolean checkMail()
        {
            DB db = new DB();

            DataTable table = new DataTable();

            MySqlDataAdapter adapter = new MySqlDataAdapter();

            MySqlCommand command = new MySqlCommand("SELECT * FROM `users` WHERE `mail` = @mU", db.getConnection());
            command.Parameters.Add("@mU", MySqlDbType.VarChar).Value = textBoxMail.Text;

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count > 0)
            {
                MessageBox.Show("Такой e-mail уже указан");
                return true;
            }
            else
                return false;
        }

        private void buttonAftor_Click(object sender, EventArgs e)
        {
            this.Hide();
            FormLogin formLogin = new FormLogin();
            formLogin.Show();
        }
    }

}
