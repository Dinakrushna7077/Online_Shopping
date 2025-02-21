
//scroll to top function block
const Scroll = () => {
    /*alert("Working");*/
    window.scrollTo({ top: 0, behavior: 'smooth' });
}
//Dissable Right Click
document.querySelector("body").addEventListener("contextmenu", (e) => {
    e.preventDefault();
});
//Confirmation Place order
const ConfirmOrder = () => {
    alert("Working");
};
var modal = document.getElementById('id01');

// When the user clicks anywhere outside of the modal, close it
window.onclick = function (event) {
    if (event.target == modal) {
        modal.style.display = "none";
    }
}
let Cancel = () => {
    modal.style.display = "none";
}

//Order Success
