using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRClimbableRung : XRBaseInteractable
{
    private Vector3 attachPoint;
    private XRBaseInteractor attachedInteractor;
    private CharacterController character;
    private GameObject xrOrigin;

    protected override void Awake()
    {
        base.Awake();
        xrOrigin = GameObject.Find("XR Origin");
        if (xrOrigin != null)
        {
            character = xrOrigin.GetComponent<CharacterController>();
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        attachPoint = args.interactorObject.transform.position;
        attachedInteractor = args.interactorObject.transform.GetComponent<XRBaseInteractor>();
    }

    public void Update()
    {
        if (isSelected && attachedInteractor != null && character != null)
        {
            Vector3 moveDirection = attachPoint - attachedInteractor.transform.position;
            character.Move(moveDirection);
            attachPoint = attachedInteractor.transform.position;
        }
    }
}