using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace RssBandit
{
	/// <summary>
	///ServiceContainerBase. This implementation of 
	/// IServiceContainer supports a root-node linked service distribution, 
	/// access. and removal architecture.
	/// </summary>
	internal class ServiceContainerBase:ApplicationContext , IServiceContainer
	{

		#region ivars
		// List of service instances sorted by key of service type's full name.
		private SortedList localServices = new SortedList();               
		// List that contains the Type for each service sorted by each 
		// service type's full name.
		private SortedList localServiceTypes = new SortedList();           

		// The parent IServiceContainer, or null.
		private IServiceContainer parentServiceContainer;
		#endregion

		#region ctor's
		public ServiceContainerBase():this(null) {	}
		public ServiceContainerBase(IServiceContainer parentContainer) {
			this.ServiceParent = parentContainer;
		}
		#endregion

		#region public members/properties
		public IServiceContainer ServiceParent {
			get {
				return parentServiceContainer;
			}
			set {
				parentServiceContainer = value;
				// Move any services to parent.
				for( int i=0; i<localServices.Count; i++ )
					parentServiceContainer.AddService(
						(Type)localServiceTypes.GetByIndex(i), 
						localServices.GetByIndex(i));
				localServices.Clear();
				localServiceTypes.Clear();
			}
		}

		#endregion

		#region private members/properties
		#endregion

		#region IServiceContainer Members

		public void RemoveService(Type serviceType, bool promote) {
			if( localServices[serviceType.FullName] != null ) {
				localServices.Remove(serviceType.FullName);
				localServiceTypes.Remove(serviceType.FullName);
			}
			if( promote ) {
				if( parentServiceContainer != null )
					parentServiceContainer.RemoveService(serviceType);
			}
		}

		void System.ComponentModel.Design.IServiceContainer.RemoveService(Type serviceType) {
			RemoveService(serviceType, true);
		}

		void System.ComponentModel.Design.IServiceContainer.AddService(Type serviceType, System.ComponentModel.Design.ServiceCreatorCallback callback) {
			AddService(serviceType, callback, true);
		}

		public void AddService(Type serviceType, System.ComponentModel.Design.ServiceCreatorCallback callback, bool promote) {
			if( promote && parentServiceContainer != null )            
				parentServiceContainer.AddService(serviceType, callback, true);            
			else {
				localServiceTypes[serviceType.FullName] = serviceType;
				localServices[serviceType.FullName] = callback;
			}
		}

		void System.ComponentModel.Design.IServiceContainer.AddService(Type serviceType, object serviceInstance) {
			if( parentServiceContainer != null )            
				parentServiceContainer.AddService(serviceType, serviceInstance, true);            
			else {
				localServiceTypes[serviceType.FullName] = serviceType;
				localServices[serviceType.FullName] = serviceInstance;
			}
		}

		void System.ComponentModel.Design.IServiceContainer.AddService(Type serviceType, object serviceInstance, bool promote) {
			if( promote && parentServiceContainer != null )            
				parentServiceContainer.AddService(serviceType, serviceInstance, true);            
			else {
				localServiceTypes[serviceType.FullName] = serviceType;
				localServices[serviceType.FullName] = serviceInstance;
			}
		}


		#endregion

		#region IServiceProvider Members

		public object GetService(Type serviceType) {
			if (serviceType == null)
				return null;

			if( parentServiceContainer != null )
				return parentServiceContainer.GetService(serviceType);            

			object serviceInstance = localServices[serviceType.FullName];
			if( serviceInstance == null )
				return null;
			else if( serviceInstance.GetType() == typeof(ServiceCreatorCallback) ) {
				// If service instance is a ServiceCreatorCallback, invoke 
				// it to create the service.
				return ((ServiceCreatorCallback)serviceInstance)(this, serviceType);                                
			}
			return serviceInstance;
		}

		#endregion
	}
}
