using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PapelariaApp.Forms
{
    /// <summary>Funções auxiliares para montar telas e exibir mensagens padronizadas.</summary>
    internal static class UI
    {
        public static readonly Color Primaria = Color.FromArgb(30, 90, 160);
        public static readonly Color Fundo = Color.FromArgb(245, 247, 250);

        public static void Tema(Form f)
        {
            f.Font = new Font("Segoe UI", 9.5f);
            f.BackColor = Fundo;
            f.StartPosition = FormStartPosition.CenterScreen;
        }

        public static Label Lbl(string texto, int x, int y) =>
            new Label { Text = texto, Left = x, Top = y + 4, AutoSize = true };

        public static TextBox Txt(int x, int y, int w = 200) =>
            new TextBox { Left = x, Top = y, Width = w };

        public static ComboBox Combo(int x, int y, int w = 200) =>
            new ComboBox { Left = x, Top = y, Width = w, DropDownStyle = ComboBoxStyle.DropDownList };

        public static NumericUpDown Num(int x, int y, int w = 80, decimal max = 100000, int casas = 0) =>
            new NumericUpDown { Left = x, Top = y, Width = w, Maximum = max, DecimalPlaces = casas };

        public static Button Btn(string texto, int x, int y, EventHandler click, int w = 90)
        {
            var b = new Button
            {
                Text = texto, Left = x, Top = y, Width = w, Height = 28,
                FlatStyle = FlatStyle.Flat, BackColor = Primaria, ForeColor = Color.White
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += click;
            return b;
        }

        public static DataGridView Grid(int x, int y, int w, int h) =>
            new DataGridView
            {
                Left = x, Top = y, Width = w, Height = h,
                ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false, BackgroundColor = Color.White
            };

        /// <summary>Preenche um ComboBox com uma tabela (Id / texto).</summary>
        public static void Preencher(ComboBox cb, DataTable dt, string coluna)
        {
            cb.DisplayMember = coluna;
            cb.ValueMember = "Id";
            cb.DataSource = dt;
        }

        /// <summary>Id selecionado num ComboBox (0 se nada).</summary>
        public static int Id(ComboBox cb)
        {
            if (cb.SelectedValue == null) return 0;
            return int.TryParse(cb.SelectedValue.ToString(), out int id) ? id : 0;
        }

        public static string S(object o) => o == null || o == DBNull.Value ? "" : o.ToString();

        public static decimal Dec(string texto, out bool ok)
        {
            ok = decimal.TryParse(texto, out decimal d);
            return d;
        }

        public static void Info(string m) => MessageBox.Show(m, "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
        public static void Aviso(string m) => MessageBox.Show(m, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        public static void Erro(string m) => MessageBox.Show(m, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        public static bool Confirma(string m) =>
            MessageBox.Show(m, "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        /// <summary>Valida campos obrigatórios; avisa e foca o primeiro vazio.</summary>
        public static bool Obrigatorios(params (Control campo, string nome)[] campos)
        {
            foreach (var (campo, nome) in campos)
            {
                if (string.IsNullOrWhiteSpace(campo.Text))
                {
                    Aviso($"O campo \"{nome}\" é obrigatório.");
                    campo.Focus();
                    return false;
                }
            }
            return true;
        }
    }
}
