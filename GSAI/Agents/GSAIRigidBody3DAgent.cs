using Godot;
using System;

namespace GodotSteeringAI
{
    /// <summary>
    /// A specialized steering agent that updates itself every frame so the user does
    /// not have to using a RigidBody.
    /// @category - Specialized agents
    /// </summary>
    public class GSAIRigidBody3DAgent: GSAISpecializedAgent
    {
        /// <summary>
        /// The RigidBody to keep track of
        /// </summary>
        public RigidBody Body { get { return _BodyRefToBody(); } set { _SetBody(value); } }

        private WeakRef _body_ref;
        private Vector3 _last_position;

        private RigidBody _BodyRefToBody()
        {
            return _body_ref.GetRef() as RigidBody;
        }

        public GSAIRigidBody3DAgent(RigidBody body)
        {
            if (!body.IsInsideTree())
            {
                body.Connect("ready", this, nameof(_OnBody_Ready));
                return;
            }
            _SetBody(body);
            body.GetTree().Connect("physics_frame", this, nameof(_onSceneTree_PhysicsFrame));
        }

        private void _OnBody_Ready()
        {
            _SetBody(_BodyRefToBody());
            _BodyRefToBody().GetTree().Connect("physics_frame", this, nameof(_onSceneTree_PhysicsFrame));
        }

        /// <summary>
        /// Moves the agent's `body` by target `acceleration`.
        /// @tags - virtual
        /// </summary>
        /// <param name="acceleration"></param>
        /// <param name="delta"></param>
        public override void _ApplySteering(GSAITargetAcceleration acceleration, float delta)
        {
            var body = Body;
            if (body is null)
                return;

            _applied_steering = true;
            body.ApplyCentralImpulse(acceleration.Linear);
            body.ApplyTorqueImpulse(Vector3.Up * acceleration.Angular);
            if (CalculateVelocities)
            {
                LinearVelocity = body.LinearVelocity;
                AngularVelocity = body.AngularVelocity.y;
            }
        }

        private void _SetBody(RigidBody body)
        {
            if (body is null)
                return;
            _body_ref = WeakRef(body);
            _last_position = body.Transform.origin;
            _last_orientation = body.Rotation.y;

            Position = _last_position;
            Orientation = _last_orientation;
        }

        private void _onSceneTree_PhysicsFrame()
        {
            var body = Body;
            if (body is null || !body.IsInsideTree())
                return;

            var current_position = body.Transform.origin;
            var current_orientation = body.Rotation.y;

            Position = current_position;
            Orientation = current_orientation;

            if (CalculateVelocities)
            {
                if (_applied_steering)
                    _applied_steering = false;
                else
                {
                    LinearVelocity = body.LinearVelocity;
                    AngularVelocity = body.AngularVelocity.y;
                }
            }
        }
    }
}
