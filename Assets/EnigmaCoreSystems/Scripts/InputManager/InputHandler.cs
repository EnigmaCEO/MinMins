namespace Enigma.CoreSystems
{
	using UnityEngine;
	using System.Collections;

	public class InputHandler : MonoBehaviour 
	{
		public bool subscribed = false;
		// Use this for initialization
		protected virtual void Start () 
		{
			//Collider collider = this.gameObject.GetComponent<Collider>();
			//if( collider == null )
			//{
			//	NGUITools.AddWidgetCollider(this.gameObject);
			//}
		}

		protected virtual void OnEnable()
		{
			if( InputManager.Instance != null )
			{
				Subscribe ();
				subscribed = true;
			}
		}

		protected virtual void OnDisable()
		{
			if( subscribed )
			{
				Unsubscribe();
				subscribed = false;
			}
		}
		
		// Update is called once per frame
		protected virtual void Update () 
		{
			if( !subscribed )
			{
				Subscribe ();
				subscribed = true;
			}
		}

		protected virtual void Subscribe()
		{

		}

		protected virtual void Unsubscribe()
		{

		}
	}
}