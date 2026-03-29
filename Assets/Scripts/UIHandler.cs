using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIHandler : MonoBehaviour
{
    public static UIHandler Instance;

    [SerializeField] TextMeshProUGUI textmesh;

    [SerializeField] TMP_InputField idInput;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static UnityEvent<FixedString128Bytes, int> sendMessage = new UnityEvent<FixedString128Bytes, int>();

    InputSystem_Actions inputs;
    private void OnEnable()
    {
        inputs = new InputSystem_Actions();
        inputs.Enable();
        inputs.UI.Submit.started += OnEnterPressed;
    }

    void OnEnterPressed(InputAction.CallbackContext obj)
    {
        int idInputValue = int.TryParse(idInput.text, out _) ? int.Parse(idInput.text) : 0;
        if(this.GetComponent<TMP_InputField>().text.ToString() == "")
        {
            return;
        }
        sendMessage.Invoke(this.GetComponent<TMP_InputField>().text.ToString(), idInputValue);
    }

    public void ShowMessage(FixedString128Bytes message, FixedString128Bytes sender)
    {
        Debug.Log(sender + ": " + message);
        textmesh.GetComponent<TMP_Text>().text = message + ": " + sender;
    }
}
