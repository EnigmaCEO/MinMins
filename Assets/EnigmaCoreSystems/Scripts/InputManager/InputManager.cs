using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enigma.CoreSystems;

public delegate void InputManagerTouchEvent( Gesture g );
public enum TouchEvent
{
	ON_TOUCH_START,
	ON_TOUCH_DOWN,
	ON_TOUCH_END,
	ON_DRAG_START,
	ON_DRAG,
	ON_DRAG_END,
	ON_SWIPE_START,
	ON_SWIPE,
	ON_SWIPE_END,
	ON_PINCH_IN,
	ON_PINCH_OUT,
	ON_PINCH_END,
	ON_TWIST,
	ON_TWIST_END,
	ON_TOUCH_CANCEL
}

public class InputManager : Manageable<InputManager>
{
	private EasyTouch m_easyTouch;
	private static Dictionary<InputManagerTouchEvent, object> touchMapping;
	private bool mAutoSelect;
	private int mStationaryTolerance;
	private float mSwipeTolerance;
	private float mLongTapTime;
	
	protected override void Awake()
	{
		base.Awake ();
		m_easyTouch = this.gameObject.AddComponent<EasyTouch>();
		touchMapping = new Dictionary<InputManagerTouchEvent, object>();
	}

	public static void Subscribe(TouchEvent touch, InputManagerTouchEvent function)
	{
		if( touchMapping.ContainsKey (function) )
		{
			Debug.LogWarning ("InputManager has already subscribed this object to event: " + function.ToString ());
			return;
		}

		switch( touch )
		{
			case TouchEvent.ON_TOUCH_START:
			{
				EasyTouch.TouchStartHandler action = new EasyTouch.TouchStartHandler(function);
				touchMapping.Add (function, action);
				EasyTouch.On_TouchStart += action;
				break;
			}
			case TouchEvent.ON_TOUCH_DOWN:
			{
				EasyTouch.TouchDownHandler action = new EasyTouch.TouchDownHandler(function);
				touchMapping.Add (function, action);
				EasyTouch.On_TouchDown += action;
				break;
			}
			case TouchEvent.ON_TOUCH_END:
			{
				EasyTouch.TouchUpHandler action = new EasyTouch.TouchUpHandler(function);
				touchMapping.Add (function, action);
				EasyTouch.On_TouchUp += action;
				break;
			}
			case TouchEvent.ON_SWIPE_START:
			{
				EasyTouch.SwipeStartHandler action = new EasyTouch.SwipeStartHandler(function);
				touchMapping.Add (function, action);
				EasyTouch.On_SwipeStart += action;
				break;
			}
			case TouchEvent.ON_SWIPE:
			{
				EasyTouch.SwipeHandler action = new EasyTouch.SwipeHandler(function);
				touchMapping.Add (function, action);
				EasyTouch.On_Swipe += action;
				break;
			}
			case TouchEvent.ON_SWIPE_END:
			{
				EasyTouch.SwipeEndHandler action = new EasyTouch.SwipeEndHandler(function);
				touchMapping.Add (function, action);
				EasyTouch.On_SwipeEnd += action;
				break;
			}
			case TouchEvent.ON_DRAG_START:
			{
				EasyTouch.DragStartHandler action = new EasyTouch.DragStartHandler(function);
				touchMapping.Add (function, action);
				EasyTouch.On_DragStart += action;
				break;
			}
			case TouchEvent.ON_DRAG:
			{
				EasyTouch.DragHandler action = new EasyTouch.DragHandler(function);
				touchMapping.Add (function, action);
				EasyTouch.On_Drag += action;
				break;
			}
			case TouchEvent.ON_DRAG_END:
			{
				EasyTouch.DragEndHandler action = new EasyTouch.DragEndHandler(function);
				touchMapping.Add (function, action);
				EasyTouch.On_DragEnd += action;
				break;
			}
			case TouchEvent.ON_PINCH_IN:
			{
				EasyTouch.PinchInHandler action = new EasyTouch.PinchInHandler(function);
				touchMapping.Add (function, action);
				EasyTouch.On_PinchIn += action;
				break;
			}
			case TouchEvent.ON_PINCH_OUT:
			{
				EasyTouch.PinchOutHandler action = new EasyTouch.PinchOutHandler(function);
				touchMapping.Add (function, action);
				EasyTouch.On_PinchOut += action;
				break;
			}
			case TouchEvent.ON_PINCH_END:
			{
				EasyTouch.PinchEndHandler action = new EasyTouch.PinchEndHandler(function);
				touchMapping.Add (function, action);
				EasyTouch.On_PinchEnd += action;
				break;
			}
			case TouchEvent.ON_TWIST:
			{
				EasyTouch.TwistHandler action = new EasyTouch.TwistHandler(function);
				touchMapping.Add (function, action);
				EasyTouch.On_Twist += action;
				break;
			}
			case TouchEvent.ON_TWIST_END:
			{
				EasyTouch.TwistEndHandler action = new EasyTouch.TwistEndHandler(function);
				touchMapping.Add (function, action);
				EasyTouch.On_TwistEnd += action;
				break;
			}
		}
	}

	public static void Unsubscribe(TouchEvent touch, InputManagerTouchEvent function)
	{
		if( !touchMapping.ContainsKey(function) )
		{
			Debug.LogWarning ("InputManager did not subscribe event: " + function.ToString());
			return;
		}

		switch( touch )
		{
			case TouchEvent.ON_TOUCH_START:
			{
				EasyTouch.On_TouchStart -= (EasyTouch.TouchStartHandler) touchMapping[function];
				break;
			}
			case TouchEvent.ON_TOUCH_DOWN:
			{
				EasyTouch.On_TouchDown -= (EasyTouch.TouchDownHandler) touchMapping[function];
				break;
			}
			case TouchEvent.ON_TOUCH_END:
			{
				EasyTouch.On_TouchUp -= (EasyTouch.TouchUpHandler) touchMapping[function];
				break;
			}
			case TouchEvent.ON_SWIPE_START:
			{
				EasyTouch.On_SwipeStart -= (EasyTouch.SwipeStartHandler) touchMapping[function];
				break;
			}
			case TouchEvent.ON_SWIPE:
			{;
				EasyTouch.On_Swipe -= (EasyTouch.SwipeHandler) touchMapping[function];
				break;
			}
			case TouchEvent.ON_SWIPE_END:
			{
				EasyTouch.On_SwipeEnd -= (EasyTouch.SwipeEndHandler) touchMapping[function];
				break;
			}
			case TouchEvent.ON_DRAG_START:
			{
				EasyTouch.On_DragStart -= (EasyTouch.DragStartHandler) touchMapping[function];
				break;
			}
			case TouchEvent.ON_DRAG:
			{
				EasyTouch.On_Drag -= (EasyTouch.DragHandler) touchMapping[function];
				break;
			}
			case TouchEvent.ON_DRAG_END:
			{
				EasyTouch.On_DragEnd -= (EasyTouch.DragEndHandler) touchMapping[function];
				break;
			}
			case TouchEvent.ON_PINCH_IN:
			{
				EasyTouch.On_PinchIn -= (EasyTouch.PinchInHandler) touchMapping[function];
				break;
			}
			case TouchEvent.ON_PINCH_OUT:
			{
				EasyTouch.On_PinchOut -= (EasyTouch.PinchOutHandler) touchMapping[function];
				break;
			}
			case TouchEvent.ON_PINCH_END:
			{
				EasyTouch.On_PinchEnd -= (EasyTouch.PinchEndHandler) touchMapping[function];
				break;
			}
			case TouchEvent.ON_TWIST:
			{
				EasyTouch.On_Twist -= (EasyTouch.TwistHandler) touchMapping[function];
				break;
			}
			case TouchEvent.ON_TWIST_END:
			{
				EasyTouch.On_TwistEnd -= (EasyTouch.TwistEndHandler) touchMapping[function];
				break;
			}
		}

		touchMapping.Remove (function);
	}

	public static void EnableAutoSelect()
	{
		Instance.m_easyTouch.autoSelect = true;
	}

	public static void DisableAutoSelect()
	{
		Instance.m_easyTouch.autoSelect = false;
	}

	public static void AddCamera(Camera cam)
	{
		Instance.m_easyTouch.touchCameras.Add ( new ECamera(cam, false) );
	}

	public static void ClearCameras()
	{
		Instance.m_easyTouch.touchCameras.Clear ();
	}

	public static void SetLayerMask( int mask = 1 )
	{
		Instance.m_easyTouch.pickableLayers = mask;
	}
}
