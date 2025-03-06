let index = 1;
const urlParams = new URLSearchParams(window.location.search);
const employeeId = urlParams.get("id");
fetchReceipt()

async function fetchReceipt()
{
    try
    {
        const response = await fetch(`http://localhost:5143/Receipt/Get?receiptId=${employeeId}`);
        const data = await response.json();
        render(data);
    } catch (error)
    {
        console.error("Error loading categories:", error);
    }
}

function render(data)
{
    document.querySelector(".receiptId").value = data.receiptName;
    document.querySelector(".date").value = data.date;
    document.querySelector(".total").value = data.totalAmount;
    document.querySelector(".discount").value = data.discount;
    document.querySelector(".grandTotal").value = data.totalValue;

    data.receiptDetail.forEach(receiptDetail =>
    {
        const tempReceipt = document.getElementById("tempReceipt");
        const tempReceiptClone = tempReceipt.content.cloneNode(true);
        tempReceiptClone.querySelector(".ReceiptId").innerText = index;
        if (receiptDetail.isInstance == true)
        {
            tempReceiptClone.querySelector(".instanceType").innerText = "ทรัพย์สินที่มีอยู่"
        }
        else
        {
            tempReceiptClone.querySelector(".instanceType").innerText = "ทรัพย์สินใหม่"
        }
        tempReceiptClone.querySelector(".instance").innerText = receiptDetail.instanceName;
        tempReceiptClone.querySelector(".price").innerText = receiptDetail.price;
        tempReceiptClone.querySelector(".quantity").innerText = receiptDetail.quantity;
        tempReceiptClone.querySelector(".totalValue").innerText = receiptDetail.totalValue;
        tempReceiptClone.querySelector(".unit").innerText = receiptDetail.unit

        const tbody = document.getElementById("TableBody")
        tbody.appendChild(tempReceiptClone);
        index++
    })
}