using System.Data;
using System.Linq;
using System.Text;

namespace Togner.Common.Tools
{
	public static class DataTableExtensions
	{
        /// <summary>
        /// Returns a string that represents the row.
        /// </summary>
        /// <param name="source">The row to convert to string.</param>
        /// <param name="itemSeparator">Separator string to insert between values of cells.</param>
        /// <returns>Strin representation on success, null otherwise.</returns>
        public static string ToString(this DataRow source, object itemSeparator)
        {
            if (source == null || source.RowState == DataRowState.Deleted || source.RowState == DataRowState.Detached)
            {
                return null;
            }

            string _result = source.Table.Columns.Cast<DataColumn>()
                .Aggregate(new StringBuilder(), (builder, colName) => builder.Append(source[colName], itemSeparator), builder => builder.ToString());

            return _result;
        }

        /// <summary>
        /// Pivot table by pivotColumn, placing pivotValues into separate columns.
        /// The rest of source table columns must have values (not null) because they
        /// will be the PK of the resulting table.
        /// In case of duplicated rows (same PK, pivotColumn but different pivotValues)
        /// no aggregation is performed - we take the latest pivotValue.
        /// </summary>
        /// <param name="source">The source table to pivot.</param>
        /// <param name="pivotColumn">The column to pivot by. The distinct values in this column will become new columns the resulting table.</param>
        /// <param name="pivotValue">The column storing values for the new pivoted columns.</param>
        /// <returns>New table pivoted by pivotColumn.</returns>
        public static DataTable Pivot(this DataTable source, DataColumn pivotColumn, DataColumn pivotValue)
        {
            if (source == null || source.Rows.Count == 0 || pivotColumn == null || pivotValue == null)
            {
                return source;
            }

            // Take distinct values of all columns besides the pivot ones
            var result = source.Copy();
            result.Columns.Remove(pivotColumn.ColumnName);
            result.Columns.Remove(pivotValue.ColumnName);
            var keyNames = result.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
            result = result.DefaultView.ToTable(true, keyNames);
            result.PrimaryKey = result.Columns.Cast<DataColumn>().ToArray();

            // Add pivoted columns
            source.AsEnumerable()
                .Select(r => r[pivotColumn.ColumnName].ToString())
                .Distinct().ToList()
                .ForEach(c => result.Columns.Add(c, pivotValue.DataType));

            // Populate pivoted columns
            foreach (DataRow row in source.Rows)
            {
                var aggRow = result.Rows.Find(keyNames.Select(k => row[k]).ToArray());

                // No aggregation - just LATEST
                aggRow[row[pivotColumn.ColumnName].ToString()] = row[pivotValue.ColumnName];
            }
            return result;
        }
	}
}
