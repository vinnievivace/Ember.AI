using UnityEngine;

namespace EmberAI.Core.Util
{
    public static class TransformUtil
    {
        #region Rotation ...............................................................................................

        /// <summary>
        /// Rotate the source towards the target incrementally (needs to be called in Update or similar)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="speed"></param>
        /// <param name="onlyLateral">Should the y-axis be ignored?</param>
        public static void RotateTowards(Component source, Component target, float speed = 1, bool onlyLateral = true)
        {
            var targetDirection = target.transform.position - source.transform.position;
            var singleStep = speed * Time.deltaTime;

            if (onlyLateral) targetDirection.y = 0;
            
            var direction = Vector3.RotateTowards(source.transform.forward, targetDirection, singleStep, 0);
            
            source.transform.rotation = Quaternion.LookRotation(direction);
        }

        /// <summary>
        /// Rotate the source to the target in a single step.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="speed"></param>
        /// <param name="onlyLateral">Should the y-axis be ignored?</param>
        public static void RotateTo(UnityEngine.Component source, UnityEngine.Component target, bool onlyLateral = true)
        {
            var targetDirection = target.transform.position - source.transform.position;
            
            if (onlyLateral) targetDirection.y = 0;
            
            var direction = Vector3.RotateTowards(source.transform.forward, targetDirection, 1000, 0);
            
            source.transform.rotation = Quaternion.LookRotation(direction);
        }
        
        /// <summary>
        /// Returns a <see cref="Quaternion"/>, adding Rotation b to Rotation a
        /// </summary>
        /// <returns></returns>
        /// <remarks>This method handles Quaternion.identity, ensuring if either value has no rotation, the other value is returned,
        /// rather than multiplying it by zero</remarks>
        public static Quaternion AddRotation(Quaternion a, Quaternion b)
        {
            if (a.eulerAngles == Vector3.zero) return b;
            if (b.eulerAngles == Vector3.zero) return a;

            return a * b;
        }

        /// <summary>
        /// Adds two rotations represented as Euler angles (in degrees) component-wise and normalizes each component to the range [0, 360).
        /// </summary>
        /// <param name="a">The first rotation represented as a Vector3 of Euler angles.</param>
        /// <param name="b">The second rotation represented as a Vector3 of Euler angles.</param>
        /// <returns>A Vector3 representing the result of adding the two rotations, with each component normalized to the range [0, 360).</returns>
        public static Vector3 AddRotation(Vector3 a, Vector3 b)
        {
            // Add the two rotations (Euler angles) component-wise
            Vector3 result = a + b;

            // Normalize or wrap the resulting angles to ensure they remain within 0-360 degrees
            result.x = NormalizeAngle(result.x);
            result.y = NormalizeAngle(result.y);
            result.z = NormalizeAngle(result.z);

            return result;
        }

        /// <summary>
        /// Normalize an angle (in degrees) to the range [0, 360)
        /// </summary>
        private static float NormalizeAngle(float angle)
        {
            return (angle % 360 + 360) % 360;
        }

        #endregion
        
        #region Position ...............................................................................................

        /// <summary>
        /// Returns a <see cref="Vector3"/> at the specified distance, in front of the supplied <see cref="UnityEngine.Component"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector3 GetPositionInFront(Component source, float distance, float yOffset = 0)
        {
            return source.transform.position + (source.transform.forward * distance) + new Vector3(0,yOffset, 0);
        }

        /// <summary>
        /// Positions the target relative to the source in the specified direction and distance.
        /// </summary>
        /// <param name="source">The source component used as the reference for positioning.</param>
        /// <param name="target">The target component to be positioned and rotated.</param>
        /// <param name="targetRotation">The rotation to apply to the target component.</param>
        /// <param name="targetOffset">The directional offset from the source.</param>
        /// <param name="targetDistance">The distance from the source to the target.</param>
        public static void PositionRelativeTo(Component source, Component target, Quaternion targetRotation, Vector3 targetOffset, float targetDistance)
        {
            Vector3 offsetDirection = targetRotation * targetOffset.normalized;
            Vector3 offsetPosition = source.transform.position + offsetDirection * targetDistance;

            target.transform.position = offsetPosition;
            target.transform.rotation = targetRotation; 
        }
        
        #endregion
        
        #region Scale ..................................................................................................

        /// <summary>
        /// Scales the target to the desired scale, relative to the parent
        /// </summary>
        /// <param name="target"></param>
        /// <param name="scale"></param>
        public static void SetRelativeScale(GameObject target, Vector3 scale)
        {
            SetRelativeScale(target, target.transform.parent, scale);    
        }
        
        public static void SetRelativeScale(GameObject target, Transform parent, Vector3 scale)
        {
            Vector3 parentScale = parent.lossyScale;
            
            target.transform.localScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);    
        }
        
        #endregion
        
        /// <summary>
        /// Get Center of the Target, if it has a collider from which bounds can be retrieved, otherwise returns the Position
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetCenter(UnityEngine.Component target)
        {
            if(target == null) return Vector3.zero;
			
            var targetCollider = target.GetComponent<Collider>();

            return targetCollider == null ? target.transform.position : targetCollider.bounds.center;
        }
        
        public static Bounds GetBounds(GameObject target, bool includeChildren)
        {
            Renderer mainRenderer = target.GetComponent<Renderer>();

            if (mainRenderer == null && !includeChildren)
            {
                throw new System.NullReferenceException("Target does not have a Renderer component.");
            }

            Bounds bounds = new Bounds();
            bool boundsInitialized = false;

            if (mainRenderer != null)
            {
                bounds = mainRenderer.bounds;
                boundsInitialized = true;
            }

            if (includeChildren)
            {
                Renderer[] childRenderers = target.GetComponentsInChildren<Renderer>();

                foreach (Renderer renderer in childRenderers)
                {
                    if (!boundsInitialized)
                    {
                        bounds = renderer.bounds;
                        boundsInitialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }

            if (!boundsInitialized)
            {
                throw new System.NullReferenceException("Target and its children do not have any Renderer components.");
            }

            return bounds;
        }
        
        public static bool IsWithinBounds(GameObject container, GameObject target)
        {
            Bounds containerBounds = GetBounds(container, true);
            Bounds targetBounds = GetBounds(target, true);

            return containerBounds.Contains(targetBounds.center) ;
        }
        
        public static float GetProximity(UnityEngine.Component target1, UnityEngine.Component target2)
        {
            if (target1 == null || target2 == null)
            {
                throw new System.Exception("One or both source components are null.");
            }

            // Ensure both components have a Transform component
            Transform transform1 = target1.transform;
            Transform transform2 = target2.transform;

            if (transform1 == null || transform2 == null)
            {
                throw new System.Exception("One or both components do not have a Transform.");
            }

            // Calculate and return the distance between the two positions
            return Vector3.Distance(transform1.position, transform2.position);
        }


        
    }
}