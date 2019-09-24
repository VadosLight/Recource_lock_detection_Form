using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Recource_lock_detection_Form
{
	public partial class Form1 : Form
	{
		uint[] available_Res;
		uint[,] allocated_Res;
		private int max_size_of_field = 3;
		public Form1()
		{
			InitializeComponent();
			dataGridView1.Rows.Add();
			dataGridView2.Rows.Add();
		}
		private void btn_add_Res_Click(object sender, EventArgs e)
		{
			try
			{
				if (dataGridView1.ColumnCount <= 9)
				{
					var column = new DataGridViewColumn(dataGridView1[0, 0]);
					var column2 = new DataGridViewColumn(dataGridView2[0, 0]);
					column.Width = 45;
					column2.Width = 45;
					
					var column_count = dataGridView1.ColumnCount + 1;
					column.HeaderText = "R" + column_count;
					column2.HeaderText = "R" + column_count;
					

					dataGridView1.Columns.Add(column);
					dataGridView2.Columns.Add(column2);

					DataGridViewTextBoxColumn cName = (DataGridViewTextBoxColumn)dataGridView1.Columns[column_count-1];
					cName.MaxInputLength = max_size_of_field;
					DataGridViewTextBoxColumn cName2 = (DataGridViewTextBoxColumn)dataGridView2.Columns[column_count-1];
					cName2.MaxInputLength = max_size_of_field;
				}
			}
			catch (Exception)
			{
			//Ничего не делаем
			}
		}
		private void btn_del_Res_Click(object sender, EventArgs e)
		{
			try
			{
				var RowIndex = 0;
				int ColumnIndex = dataGridView1.ColumnCount - 1;

				dataGridView1.ClearSelection();
				dataGridView2.ClearSelection();

				dataGridView1.Rows[RowIndex].Cells[ColumnIndex].Selected = true;
				dataGridView1.CurrentCell = dataGridView1.Rows[RowIndex].Cells[ColumnIndex];

				dataGridView2.Rows[RowIndex].Cells[ColumnIndex].Selected = true;
				dataGridView2.CurrentCell = dataGridView2.Rows[RowIndex].Cells[ColumnIndex];

				if (dataGridView1.SelectedCells.Count != 0)
					if (dataGridView1.SelectedCells[0].ColumnIndex > 2)
					{
						dataGridView1.Columns.RemoveAt(dataGridView1.SelectedCells[0].ColumnIndex);
						dataGridView2.Columns.RemoveAt(dataGridView2.SelectedCells[0].ColumnIndex);
					}
			}
			catch
			{ }

		}
		private void btn_Remove_Row_Click(object sender, EventArgs e)
		{
			//Удаляем предпоследнюю строку в левой таблице
			if (dataGridView1.RowCount >= 3)
				dataGridView1.Rows.RemoveAt(dataGridView1.RowCount - 2); 
		}

		async void button1_Click(object sender, EventArgs e)
		{
			try
			{
				//Проверка левого поля на корректность данных (Буквы, пустые ячейки)
				for (int i = 0; i < dataGridView1.RowCount - 1; i++)
					for (int j = 1; j < dataGridView1.ColumnCount; j++)
					{
						if (!uint.TryParse(dataGridView1.Rows[i].Cells[j].FormattedValue.ToString(), out uint d))
						{
							MessageBox.Show("Проверьте корректность введённых данных левого поля.");
							return;
						}
					}
				//Проверка правого поля на корректность данных (Буквы, пустые ячейки)
				for (int i = 0; i < dataGridView2.RowCount; i++)
					for (int j = 1; j < dataGridView2.ColumnCount; j++)
					{
						if (!uint.TryParse(dataGridView2.Rows[i].Cells[j].FormattedValue.ToString(), out uint d))
						{
							MessageBox.Show("Проверьте корректность введённых данных правого поля.");
							return;
						}
					}
				//Собираем данные из таблиц
				available_Res = new uint[dataGridView2.ColumnCount];
				allocated_Res = new uint[dataGridView1.RowCount - 1, dataGridView1.ColumnCount];
				//Из левой таблицы
				for (int i = 0; i < dataGridView1.RowCount - 1; i++)
				{
					for (int j = 0; j < dataGridView1.ColumnCount ; j++)
					{						
						allocated_Res[i, j] = Convert.ToUInt16(dataGridView1.Rows[i].Cells[j].FormattedValue);
						//MessageBox.Show("" + allocated_Res[i, j]);
					}
				}
				//Из правой таблицы
				for (int j = 0; j < dataGridView2.ColumnCount; j++)
				{
					available_Res[j] = Convert.ToUInt16(dataGridView2.Rows[0].Cells[j].FormattedValue);
				}
				//Поиск тупиков - тупик здесь только кодер
				richTextBox1.Text = "";
				bool flag = true;
				uint count_of_good_steps = 0;
				for (int step = 0; step < dataGridView1.RowCount - 1; step++)//Счётчик полных проходов по таблице
				{					
					for (int i = 0; i < dataGridView1.RowCount - 1; i++)//Счётчик, который указывает на текщую строку
					{
						for (int j = 0; j < dataGridView1.ColumnCount; j++)//Счётчик, который указывает на текущий элемент строки
						{
							if (allocated_Res[i, j] <= available_Res[j])
							{
								if (j == dataGridView1.ColumnCount - 1)
								{
									await Task.Delay(500);
									++count_of_good_steps;
									//Выводим логи для Пользователя
									richTextBox1.AppendText("Высвобождаются ресурсы: ");

									for(int l = 0; l < dataGridView1.ColumnCount; l++)
										richTextBox1.AppendText("["+ allocated_Res[i,l] +"] ");
									richTextBox1.AppendText("\n");

									//Вершится магия
									for (int l = 0; l < dataGridView1.ColumnCount; l++)
									{
										uint tmp1 = available_Res[l];
										uint tmp2 = allocated_Res[i, l];

										available_Res[l] = tmp1 + tmp2;
										allocated_Res[i,l] = 99999999;
									}

									richTextBox1.AppendText("Доступные ресурсы: ");
									for (int l = 0; l < dataGridView2.ColumnCount; l++)
									{
										richTextBox1.AppendText("[" + available_Res[l] + "] ");
									}
									richTextBox1.AppendText("\n");
								}
							}
							else
							{
								break;
							}
						}
					}
				}
				if (count_of_good_steps < (dataGridView1.RowCount - 1))
				{
					flag = false;

					richTextBox1.AppendText("\nВозникла тупиковая ситуация на " + count_of_good_steps
						+ "-м шаге");					
				}
				if (flag)
				{
					richTextBox1.AppendText("\nПоздравляем! Тупиковых ситуаций не обнаружено\n"
						+ "Все процессы выполнены, ресурсы распределены.");
				}
			}
			catch(Exception exc)
			{
				Console.WriteLine(exc.Message);				
				//MessageBox.Show(exc.Message);
			}
		}
	}
}
