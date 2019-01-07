using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IqaController
{
    class DataGridViewEx : DataGridView
    {
        public DataGridViewEx()
        {
            this.AllowUserToAddRows = false;
            this.RowCount = 0;
            this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            this.ColumnHeadersHeight = 23;
            this.ReadOnly = true;
        }

        /* 해당 cell을 기본 Text 형식으로 설정한다.
         * 
        */
        public void setCell(string value, int col, int row)
        {
            DataGridViewRow r = this.Rows[row];
            r.Cells[col] = new DataGridViewTextBoxCell();            
            ((DataGridViewTextBoxCell)r.Cells[0]).Value = value;            
        }
        /* 해당 cell을 Button 형식으로 설정한다.
         * 
        */
        public void setCellButton(string value, int col,int row)
        {
            DataGridViewRow r = this.Rows[row];
            r.Cells[col] = new DataGridViewButtonCell();
            ((DataGridViewButtonCell)r.Cells[col]).Value = value;
        }

        public void addColumn(string headerText, int width)
        {
            DataGridViewTextBoxColumn tmp = new DataGridViewTextBoxColumn();
            //tmp.Name = "항목";
            tmp.HeaderText = headerText;
            tmp.Width = width;
            tmp.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;                        
            this.Columns.Add(tmp);
        }

        public void addColumnCheckBox(string headerText,bool selected, int width)
        {
            DataGridViewCheckBoxColumn tmp = new DataGridViewCheckBoxColumn();
            tmp.HeaderText = headerText;
            tmp.Width = width;
            //tmp.Selected(true);
            this.Columns.Add(tmp);
        }

        public void addColumnButton(string headerText, string buttonText, int width)
        {
            DataGridViewButtonColumn tmp = new DataGridViewButtonColumn();
            tmp.HeaderText = headerText;
            tmp.Text = buttonText;
            tmp.Width = width;
            tmp.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //btn.Name = "btn";
            tmp.UseColumnTextForButtonValue = true;
            this.Columns.Add(tmp);
        }

        public int getFindZipFile2RowIndex(string zipFileNm, int nCol)
        {
            int nRow = -1;
            foreach (DataGridViewRow r in this.Rows)
            {
                string tmpZipFileName = (string)((DataGridViewTextBoxCell)r.Cells[nCol]).Value;

                if (tmpZipFileName == zipFileNm)
                {
                    nRow = r.Index;
                    break;
                }
            }
            return nRow;
        }



    }
}
