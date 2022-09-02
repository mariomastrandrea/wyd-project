using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace _13_Wyd.ModelClasses
{
    public class StringToItemConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        //string --> Item
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if(value is string)
            {
                string stringToConvert = (string)value;
                string[] itemFields = stringToConvert.Split(";");

                if (itemFields.Length != 8) return null;

                List<Tuple<string, string>> fieldsPairs = itemFields.Select(field =>
                {
                    string[] pair = field.Split("=", 2);
                    return pair.Length == 2 ? new Tuple<string, string>(pair[0].Trim(), pair[1].Trim()) : null;
                }).ToList();

                if (fieldsPairs.Contains(null)) return null;

                if (!fieldsPairs[0].Item1.ToLower().Equals("id") ||
                   !fieldsPairs[1].Item1.ToLower().Equals("name") ||
                   !fieldsPairs[2].Item1.ToLower().Equals("cost") ||
                   !fieldsPairs[3].Item1.ToLower().Equals("seller") ||
                   !fieldsPairs[4].Item1.ToLower().Equals("image") ||
                   !fieldsPairs[5].Item1.ToLower().Equals("price") ||
                   !fieldsPairs[6].Item1.ToLower().Equals("period") ||
                   !fieldsPairs[7].Item1.ToLower().Equals("categories"))
                    return null;

                string id = fieldsPairs[0].Item2.Equals(string.Empty) ? null : fieldsPairs[0].Item2;
                string name = fieldsPairs[1].Item2.Equals(string.Empty) ? null : fieldsPairs[1].Item2;
                decimal? cost = fieldsPairs[2].Item2.Equals(string.Empty) ? null : decimal.Parse(fieldsPairs[2].Item2);
                string seller = fieldsPairs[3].Item2.Equals(string.Empty) ? null : fieldsPairs[3].Item2;
                string image = fieldsPairs[4].Item2.Equals(string.Empty) ? null : fieldsPairs[4].Item2;
                decimal? price = fieldsPairs[5].Item2.Equals(string.Empty) ? null : decimal.Parse(fieldsPairs[5].Item2);
                TimeSpan? period;
                IEnumerable<string> categories;

                try
                {
                    period = fieldsPairs[6].Item2.Equals(string.Empty) ? null : TimeSpan.Parse(fieldsPairs[6].Item2);
                    categories = fieldsPairs[7].Item2.Equals(string.Empty) ? null :
                                    fieldsPairs[7].Item2.Split(",").Select(s => s.Trim());
                }
                catch (Exception)
                {
                    return null;
                }

                return new Item(id, name, cost, seller, image, price, period, categories);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if(destinationType == typeof(string))
            {
                return ((Item)value).ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
