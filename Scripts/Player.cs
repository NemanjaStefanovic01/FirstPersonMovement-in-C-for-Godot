//First person movement C# script for Godot based on Lukky(YT) script in GDscript

using Godot;
using System;
using System.Security;
using System.Text.RegularExpressions;

public partial class Player : CharacterBody3D
{
	//Movement forces
	public float cuerrentSpeed = 5f;
	[Export]
	public float walkingSpeed = 6f;
	[Export]
	public float sprintingSpeed = 7f;
	[Export]
	public float crouchingSpeed = 3f;
	[Export]
	public float jumpVelocity = 4.5f;
	//adding "drag"
	[Export]
	public float lerpSpeed = 10f;
	public Vector3 direction = Vector3.Zero;
	[Export]
	//Crouching
	public float crouchingDepth = 0.5f;
	public Vector3 headPositionWhenNotCrouched = Vector3.Zero;
	public Vector3 headPositionWhenCrouched = Vector3.Zero;

	//Mouse
	[Export]
	public float mouseSens = 1f;
	public float mouseSensMultiplier = 0.1f;

	//Refs
	public Node3D head;
	public Camera3D cam;
	public CollisionShape3D colStanding;
	public CollisionShape3D colCrouching;
	public RayCast3D rayCast;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		//Get references of noted for head and cam
		head = GetNode<Node3D>("Head");
		cam = GetNode<Camera3D>("Head/Camera3D");
		//Ref to collision shapes
		colStanding = GetNode<CollisionShape3D>("Standing ColisionShape");
		colCrouching = GetNode<CollisionShape3D>("Crouching ColisionShape");
		//Ref to raycast
		rayCast = GetNode<RayCast3D>("RayCast3D");

		//Setting up head postitions for crouching and standing up
		headPositionWhenNotCrouched = head.Position;
		headPositionWhenCrouched = head.Position - new Vector3(0, crouchingDepth, 0);

		//Hides and locks the cursor
		DisplayServer.MouseSetMode(DisplayServer.MouseMode.Captured);
	}

	public override void _Input(InputEvent @event)
	{
		//Look around using mouse
		if(@event is InputEventMouseMotion)
		{
			InputEventMouseMotion mouseMotion = @event as InputEventMouseMotion;

			//Rotate on X (obj and head)
			RotateY(Mathf.DegToRad(-mouseMotion.Relative.X * mouseSens * mouseSensMultiplier));//DegToRad converts deegres to radians 
			head.RotateX(Mathf.DegToRad(-mouseMotion.Relative.Y * mouseSens * mouseSensMultiplier));
			
			//Lock rotation on Y
			Vector3 headRot = head.Rotation;
			headRot.X = Mathf.Clamp(head.Rotation.X, Mathf.DegToRad(-89), Mathf.DegToRad(89));
			head.Rotation = headRot;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		//Handling movement states
		if(Input.IsActionPressed("crouch"))
		{
			head.Position = LerpVec3(headPositionWhenCrouched, headPositionWhenNotCrouched, (float)delta*lerpSpeed);

			cuerrentSpeed = crouchingSpeed;
			colStanding.Disabled = true;
			colCrouching.Disabled = false;
		}
		else if(!rayCast.IsColliding())
		{
			head.Position = LerpVec3(headPositionWhenNotCrouched, headPositionWhenCrouched, (float)delta*lerpSpeed);

			colStanding.Disabled = false;
			colCrouching.Disabled = true;

			if(Input.IsActionPressed("sprint")){
				cuerrentSpeed = sprintingSpeed;
			}else{
				cuerrentSpeed = walkingSpeed;
			}
		}

		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = jumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		//Make movement smooth using lerp
		direction = LerpVec3(direction, (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized(), (float)delta * lerpSpeed);

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * cuerrentSpeed;
			velocity.Z = direction.Z * cuerrentSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, cuerrentSpeed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, cuerrentSpeed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	//Misc
	public Vector3 LerpVec3(Vector3 from, Vector3 to, float by)
	{
		float retX = Mathf.Lerp(from.X, to.X, by);
		float retY = Mathf.Lerp(from.Y, to.Y, by);
		float retZ = Mathf.Lerp(from.Z, to.Z, by);

		return new Vector3(retX, retY, retZ);
	}
}
