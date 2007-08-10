#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;

namespace System.Windows.Forms.ThListView
{
	internal class ThreadedListViewItemCollectionEditor : System.ComponentModel.Design.CollectionEditor { 

		public ThreadedListViewItemCollectionEditor() : base(typeof(ThreadedListViewItemCollection)) { 
		} 

		protected override object CreateInstance(System.Type itemType) { 
			return new ThreadedListViewItem(); 
		} 

		protected override System.Type CreateCollectionItemType() { 
			return typeof(ThreadedListViewItem); 
		} 
	} 
	internal class ThreadedListViewItemConverter : ExpandableObjectConverter { 

		public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType) { 
			if (destinationType == typeof(InstanceDescriptor)) { 
				return true; 
			} 
			return base.CanConvertTo(context, destinationType); 
		} 

		public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType) { 
			if (destinationType == typeof(InstanceDescriptor)) { 
				Type[] signature = {typeof(ThreadedListViewItem.ListViewSubItem[]), typeof(int), typeof(int)}; 
				ThreadedListViewItem itm = ((ThreadedListViewItem)value); 
				object[] args = {itm.SubItemsArray, itm.ImageIndex, itm.GroupIndex}; 
				return new InstanceDescriptor(typeof(ThreadedListViewItem).GetConstructor(signature), args, false); 
			} 
			return base.ConvertTo(context, culture, value, destinationType); 
		} 
	} 
	internal class ThreadedListViewGroupConverter : ExpandableObjectConverter { 

		public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType) { 
			if (destinationType == typeof(InstanceDescriptor)) { 
				return true; 
			} 
			return base.CanConvertTo(context, destinationType); 
		} 

		public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType) { 
			if (destinationType == typeof(InstanceDescriptor)) { 
				Type[] signature = {typeof(string), typeof(int)}; 
				ThreadedListViewGroup itm = ((ThreadedListViewGroup)value); 
				object[] args = {itm.GroupText, itm.GroupIndex}; 
				return new InstanceDescriptor(typeof(ThreadedListViewGroup).GetConstructor(signature), args, false); 
			} 
			return base.ConvertTo(context, culture, value, destinationType); 
		} 
	}
}
