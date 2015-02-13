using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PowerShellTools.Project.PropertyPages
{
    public partial class ItemCollection : UserControl
    {
        public ItemCollection()
        {
            InitializeComponent();
        }

        public ItemCollection(List<string> potentialItems, List<string> addedItems)
        {
            InitializeComponent();

            potentialItems = potentialItems.Except(addedItems).ToList();

            cmoItems.Items.AddRange(potentialItems.ToArray());
            lstItems.Items.AddRange(addedItems.ToArray());

            cmoItems.TextChanged += (sender, args) =>
            {
                btnAdd.Enabled = !String.IsNullOrEmpty(cmoItems.Text);
            };

            cmoItems.SelectedIndexChanged += (sender, args) =>
            {
                btnAdd.Enabled = !String.IsNullOrEmpty(cmoItems.Text);
            };

            lstItems.SelectedIndexChanged += (sender, args) =>
            {
                btnRemove.Enabled = true;
            };
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            lstItems.Items.Add(cmoItems.Text);
            cmoItems.Items.Remove(cmoItems.Text);

            OnItemsChanged(GetItems());
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            lstItems.Items.Remove(lstItems.SelectedItem);
            cmoItems.Items.Add(lstItems.SelectedItem);

            OnItemsChanged(GetItems());
        }

        public EventHandler<IEnumerable<string>> ItemsChanged;

        private void OnItemsChanged(IEnumerable<string> items)
        {
            if (ItemsChanged != null)
                ItemsChanged(this, items);
        }

        private IEnumerable<string> GetItems()
        {
            return from object item in lstItems.Items select item.ToString();
        }
    }
}
