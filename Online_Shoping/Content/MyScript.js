
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
const sear = ["Find Me", "Redmi", "Vivo", "Oppo", "Samsung", "Laptop", "HP Paviliion", "Lenevo", "Search Here"]
let inval = 0;
let s = document.getElementById("srch");
setInterval(() => {
    inval = (inval + 1) % sear.length;
    s.placeholder = sear[inval];
}, 2000);