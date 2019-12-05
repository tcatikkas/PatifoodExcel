using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatifoodExcelApp
{
    public class Product
    {

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class items
        {

            private itemsItem itemField;

            /// <remarks/>
            public itemsItem item
            {
                get
                {
                    return this.itemField;
                }
                set
                {
                    this.itemField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class itemsItem
        {
            private int category_idField;
            
            
            /// <remarks/>
            public int category_id
            {
                get
                {
                    return this.category_idField;
                }
                set
                {
                    this.category_idField = value;
                }
            }
        }


    }
}
