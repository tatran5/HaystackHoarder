using UnityEngine;
using Photon.Pun;

public class PUN2_GasCanSync : MonoBehaviourPun, IPunObservable
{
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {}

    public void callDisappear()
    {
        photonView.RPC("disappear", RpcTarget.All);
    }

    [PunRPC]
    public void disappear()
    {
        Debug.Log("FT");
        gameObject.SetActive(false);
    }
}
