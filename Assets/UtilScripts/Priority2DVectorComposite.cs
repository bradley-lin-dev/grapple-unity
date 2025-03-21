using System.ComponentModel;
using UnityEditor;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem.Composites {
    /// <summary>
    /// A 2D planar motion vector computed from an up+down button pair as vertical and
    /// a left+right button pair as horizontal that prioritizes the latest input of that
    /// axis.
    /// </summary>
    /// <remarks>
    /// This composite allows to grab arbitrary buttons from a device and arrange them in
    /// a D-Pad like configuration.
    ///
    /// Opposing motions will prioritize the latest pressed input. So for example, if the left
    /// and and then the right horizontal button are pressed, the resulting horizontal movement
    /// value will be right.
    ///
    /// <example>
    /// <code>
    /// // Set up WASD style keyboard controls.
    /// action.AddCompositeBinding("2DVectorPriority")
    ///     .With("Up", "&lt;Keyboard&gt;/w")
    ///     .With("Left", "&lt;Keyboard&gt;/a")
    ///     .With("Down", "&lt;Keyboard&gt;/s")
    ///     .With("Right", "&lt;Keyboard&gt;/d");
    /// </code>
    /// </example>
    /// </remarks>
#if UNITY_EDITOR
    [InitializeOnLoad] // Automatically register in editor.
#endif
    [DisplayStringFormat("{up}/{left}/{down}/{right}")] // This results in WASD.
    [DisplayName("Priority Up/Down/Left/Right Composite")]
    public class Priority2DVectorComposite : InputBindingComposite<Vector2> {
        /// <summary>
        /// Binding for the button that represents the up (that is, <c>(0,1)</c>) direction of the vector.
        /// </summary>
        /// <remarks>
        /// This property is automatically assigned by the input system.
        /// </remarks>
        [InputControl(layout = "Button")] public int up;

        /// <summary>
        /// Binding for the button represents the down (that is, <c>(0,-1)</c>) direction of the vector.
        /// </summary>
        /// <remarks>
        /// This property is automatically assigned by the input system.
        /// </remarks>
        [InputControl(layout = "Button")] public int down;

        /// <summary>
        /// Binding for the button represents the left (that is, <c>(-1,0)</c>) direction of the vector.
        /// </summary>
        /// <remarks>
        /// This property is automatically assigned by the input system.
        /// </remarks>
        [InputControl(layout = "Button")] public int left;

        /// <summary>
        /// Binding for the button that represents the right (that is, <c>(1,0)</c>) direction of the vector.
        /// </summary>
        /// <remarks>
        /// This property is automatically assigned by the input system.
        /// </remarks>
        [InputControl(layout = "Button")] public int right;

        public override Vector2 ReadValue(ref InputBindingCompositeContext context) {
            bool leftValue = context.ReadValueAsButton(left);
            bool rightValue = context.ReadValueAsButton(right);
            bool downValue = context.ReadValueAsButton(down);
            bool upValue = context.ReadValueAsButton(up);

            float horizontal = 0f;
            if (leftValue && rightValue) {
                horizontal = (context.GetPressTime(right) > context.GetPressTime(left)) ? 1f : -1f;
            } else if (leftValue) {
                horizontal = -1f;
            } else if (rightValue) {
                horizontal = 1f;
            }

            float vertical = 0f;
            if (downValue && upValue) {
                vertical = (context.GetPressTime(up) > context.GetPressTime(down)) ? 1f : -1f;
            } else if (downValue) {
                vertical = -1f;
            } else if (upValue) {
                vertical = 1f;
            }

            return new Vector2(horizontal, vertical);
        }

        public override float EvaluateMagnitude(ref InputBindingCompositeContext context) {
            return ReadValue(ref context).magnitude;
        }

        static Priority2DVectorComposite() {
            InputSystem.RegisterBindingComposite<Priority2DVectorComposite>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init() {}
    }
}
