namespace echo17.EndlessBook.Demo03
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using echo17.EndlessBook;

    /// <summary>
    /// This demo shows one way you could implement manual page dragging in your book
    /// </summary>
	public class BookController : MonoBehaviour
	{

        /// <summary>
        /// Make sure the audio is off so that we don't get an open sound at the beginning
        /// </summary>
        protected bool audioOn = true;

        /// <summary>
        /// The scene camera used for ray casting
        /// </summary>
        public Camera sceneCamera;

		/// <summary>
	    /// The book to control
	    /// </summary>
		public EndlessBook book;

		/// <summary>
		/// The speed to play the page turn animation when the mouse is let go
		/// </summary>
		public float turnStopSpeed;

		/// <summary>
	    /// If this is turned on, then the page will reverse direction
		/// if the page is not past the midway point of the book.
	    /// </summary>
		public bool reversePageIfNotMidway = true;

		/// <summary>
		/// The box collider to check for mouse motions
		/// </summary>
		protected BoxCollider boxCollider;

		/// <summary>
		/// Whether the mouse is currently down
		/// </summary>
		protected bool isMouseDown;


        /// <summary>
        /// The sound to make when the book opens
        /// </summary>
        public AudioSource bookOpenSound;

        /// <summary>
        /// The sound to make when the book closes
        /// </summary>
        public AudioSource bookCloseSound;

        /// <summary>
        /// The sounds for each of the page components' turn
        /// </summary>
        public AudioSource pageTurnSound;

        /// <summary>
        /// The sound to make when multiple pages are turning
        /// </summary>
        public AudioSource pagesFlippingSound;


        void Awake()
		{
			// cache the box collider for faster referencing
			boxCollider = gameObject.GetComponent<BoxCollider>();
            Debug.Log(boxCollider);
        }

        private void Start()
        {

            // turn on the audio now that the book state is set the first time,
            // otherwise we'd hear a noise and no change would occur
          

        }

        /// <summary>
        /// Fired when the mouse intersects with the collider box while mouse down occurs
        /// </summary>
        void OnMouseDown()
		{
			Debug.Log("OnMouseDown");
            if (book.CurrentState == EndlessBook.StateEnum.ClosedFront)
			{
                book.SetState(EndlessBook.StateEnum.OpenFront);
                return;
            }

            if (book.CurrentState == EndlessBook.StateEnum.OpenFront)
            {
                
                book.SetState(EndlessBook.StateEnum.OpenMiddle);

                return;
            }


            if (book.IsTurningPages || book.IsDraggingPage || UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
			{
				// exit if already turning
				return;
			}

			// get the normalized time based on the mouse position
			var normalizedTime = GetNormalizedTime();

			// calculate the direction of the page turn based on the mouse position
			var direction = normalizedTime > 0.5f ? Page.TurnDirectionEnum.TurnForward : Page.TurnDirectionEnum.TurnBackward;

			// tell the book to start turning a page manually
			book.TurnPageDragStart(direction);

			// the mosue is now currently down
			isMouseDown = true;
		}

		/// <summary>
	    /// Fired when the mouse intersects with the collider box while dragging
	    /// </summary>
		void OnMouseDrag()
		{
			if (book.CurrentState == EndlessBook.StateEnum.ClosedFront || book.IsTurningPages || !book.IsDraggingPage || !isMouseDown || UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
			{
				// if not turning or the mouse is not down, then exit
				return;
			}

			// get the normalized time based on the mouse position
			var normalizedTime = GetNormalizedTime();

            // tell the book to move the manual page drag to the normalized time
            book.TurnPageDrag(normalizedTime);
		}

		/// <summary>
	    /// Fired when the mouse intersects with the collider and the mouse up event occurs
	    /// </summary>
		void OnMouseUp()
		{
			if (book.CurrentState == EndlessBook.StateEnum.ClosedFront || book.IsTurningPages || !book.IsDraggingPage || UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
			{
				// if not turning then exit
				return;
			}

			// tell the book to stop manual turning.
			// if we have reversePageIfNotMidway on, then we look to see if we have turned past the midway point.
			// if not, we reverse the page.
			book.TurnPageDragStop(turnStopSpeed, PageTurnCompleted, reverse: reversePageIfNotMidway ? (book.TurnPageDragNormalizedTime < 0.5f) : false);
            if (book.TurnPageDragNormalizedTime >= 0.5f)
			{
                pageTurnSound.Play();
            }
            

            // mouse is no longer down, so we can turn a new page if the animation is also completed
            isMouseDown = false;
		}

		/// <summary>
		/// Calculates the normalized time based on the mouse position
		/// </summary>
		protected virtual float GetNormalizedTime()
		{
			// get the ray from the camera to the screen
			var ray = sceneCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			// cast a ray and see where it hits
			if (Physics.Raycast(ray, out hit))
			{
				// return the position of the ray cast in terms of the normalized position of the collider box
				return (hit.point.x + (boxCollider.size.x / 2.0f)) / boxCollider.size.x;
			}

			// if we didn't hit the collider, then check to see if we are on the
			// left or right side of the screen and calculate the normalized time appropriately
			var viewportPoint = sceneCamera.ScreenToViewportPoint(Input.mousePosition);
			return (viewportPoint.x >= 0.5f) ? 1 : 0;
		}

		/// <summary>
		/// Called when the page completes its manual turn
		/// </summary>
		protected virtual void PageTurnCompleted(int leftPageNumber, int rightPageNumber)
		{
			//isTurning = false;
		}
        protected virtual void OnBookStateChanged(EndlessBook.StateEnum fromState, EndlessBook.StateEnum toState, int pageNumber)

        {
            switch (toState)
            {
                case EndlessBook.StateEnum.ClosedFront:
                case EndlessBook.StateEnum.ClosedBack:

                    // play the closed sound
                    if (audioOn)
                    {
                        bookCloseSound.Play();
                    }


                    break;

                case EndlessBook.StateEnum.OpenMiddle:

                    if (fromState != EndlessBook.StateEnum.OpenMiddle)
                    {
						// play open sound
						if (audioOn)
						{
							bookOpenSound.Play();
						}
                    }
                    else
                    {
						// stop the flipping sound
						if (audioOn)
						{
							pagesFlippingSound.Stop();
						}
                    }

                    // turn off the front and back page mini-scenes

                    break;

                case EndlessBook.StateEnum.OpenFront:
                case EndlessBook.StateEnum.OpenBack:

					// play the open sound
					if (audioOn)
					{
						bookOpenSound.Play();
					}

                    break;
            }
        }

    }

}
