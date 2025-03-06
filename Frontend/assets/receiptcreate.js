let result = {
    date: new Date().toISOString().split("T")[0],
    totalAmount: 0,
    discount: 0,
    totalValue: 0,
    receiptDetail: []
};
let index = 0;


document.addEventListener("DOMContentLoaded", function ()
{
    const modalOverlay = document.getElementById("modalOverlay");
    const categorySelect = document.getElementById("categoryId");
    const classificationSelect = document.getElementById("classificationId");
    const instanceSelect = document.getElementById("instanceId");
    const selectedInfo = document.getElementById("selectedInfo");

    categorySelect.addEventListener("change", async function ()
    {
        const categoryId = this.value;
        try
        {
            const response = await fetch(`http://localhost:5143/Item/GetClassificationByCategoryId?categoryId=${categoryId}`);
            const classifications = await response.json();
            selectedInfo.innerHTML = "";
            instanceSelect.innerHTML = "";
            classificationSelect.innerHTML = "";
            let select = document.getElementById("classificationId");
            classifications.forEach(classification =>
            {
                let option = document.createElement("option");
                option.setAttribute("value", classification.classificationId);
                option.text = classification.classificationName;
                select.appendChild(option);
            });
        } catch (error)
        {
            console.error("Error fetching classifications:", error);
        }
    });

    classificationSelect.addEventListener("change", async function ()
    {
        const classificationId = this.value;
        try
        {
            const response = await fetch(`http://localhost:5143/Item/GetInstanceByClassificationId?classificationId=${classificationId}`);
            const instances = await response.json();
            instanceSelect.innerHTML = "";
            selectedInfo.innerHTML = "";
            instances.forEach(instance =>
            {
                const li = document.createElement("li");
                li.dataset.value = instance.instanceId;
                li.textContent = instance.assetId;

                li.addEventListener("click", function ()
                {
                    document.querySelectorAll("ul .select").forEach(el => el.classList.remove("select"));
                    li.classList = "select"
                    const categoryName = categorySelect.options[categorySelect.selectedIndex].text
                    const classificationName = classificationSelect.options[classificationSelect.selectedIndex].text
                    const instanceName = this.textContent.trim();

                    const tempInfo = document.getElementById("tempInfo");
                    const tempInfoClone = tempInfo.content.cloneNode(true);
                    selectedInfo.innerHTML = "";
                    selectedInfo.appendChild(tempInfoClone);

                    document.querySelector(".categoryName").innerText = categoryName;
                    document.querySelector(".classificationName").innerText = classificationName;
                    document.querySelector(".instanceName").innerText = instanceName;
                });

                instanceSelect.appendChild(li);
            });
        } catch (error)
        {
            console.error("Error fetching instances:", error);
        }
    });


    instanceSelect.addEventListener("change", function ()
    {
        const categoryName = categorySelect.options[categorySelect.selectedIndex].text;
        const classificationName = classificationSelect.options[classificationSelect.selectedIndex].text;
        const instanceName = instanceSelect.options[instanceSelect.selectedIndex].text;

        const tempInfo = document.getElementById("tempInfo");
        const tempInfoClone = tempInfo.content.cloneNode(true);
        selectedInfo.appendChild(tempInfoClone)

        document.querySelector(".categoryName").innerText = categoryName;
        document.querySelector(".classificationName").innerText = classificationName;
        document.querySelector(".instanceName").innerText = instanceName;
    });

    document.querySelector(".btn-select").addEventListener("click", function ()
    {
        const listIndex = modalOverlay.dataset.index;
        const TrReceipt = document.querySelectorAll(".Receipt-contain")[listIndex];
        TrReceipt.querySelector(".deleteReceipt").disabled = false;
        const instanceTd = TrReceipt.querySelector(".instance");
        const SelectInstance = document.querySelector(".select")
        const instanceName = SelectInstance.innerText;
        const instanceValue = SelectInstance.dataset.value;
        instanceTd.innerText = instanceName;

        selectedInfo.innerHTML = "";
        instanceSelect.innerHTML = "";
        modalOverlay.style.display = "none";

        result.receiptDetail[listIndex].newInstance = null
        result.receiptDetail[listIndex].instanceId = parseInt(instanceValue);

        const rows = document.querySelectorAll(".Receipt-contain");
        const lastRowIndex = rows.length - 1;
        const lastRow = rows[lastRowIndex].querySelector(".instance");
        if (lastRow.hasChildNodes())
        {
            addReceipt()
        } else
            return
    });

    document.querySelector(".btn-close").addEventListener("click", function ()
    {
        modalOverlay.style.display = "none";
        selectedInfo.innerHTML = "";
        instanceSelect.innerHTML = "";
    });

    addReceipt();
});
document.querySelector(".discount").addEventListener("change", GetDiscount);
document.querySelector(".save").addEventListener("click", save)
document.querySelector(".date").value = new Date().toISOString().split("T")[0];

async function save()
{
    let lastIndex = document.querySelectorAll(".Receipt-contain").length - 1;
    let lastItem = result.receiptDetail[lastIndex];

    if (lastItem.instanceId === null && lastItem.newInstance === null && lastItem.price === 0 && lastItem.quantity === 0 && lastItem.totalValue === 0 && lastItem.unit === "string")
    {
        result.receiptDetail.splice(lastIndex, 1);
    }
    try
    {
        const response = await fetch("http://localhost:5143/Receipt/Create", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(result),
        });
        const responseData = await response.json();
        if (responseData.statusCode == 201)
        {
            alert(responseData.message || "Create completed successfully!");
            window.location.href = `/Frontend/receiptcreate.html`
        }
        else
        {
            alert(responseData.message);
        }
    } catch (error)
    {
        console.error("Error updating item detail:", error);
    }
}

async function fetchCategories()
{
    try
    {
        const response = await fetch(`http://localhost:5143/Item/GetCategory`);
        const categories = await response.json();

        const select = document.getElementById("categoryId");
        categories.forEach(categorie =>
        {
            let option = document.createElement("option");
            option.setAttribute("value", categorie.categoryId);
            option.text = categorie.categoryName;
            select.appendChild(option);
        })
    } catch (error)
    {
        console.error("Error loading categories:", error);
    }
}

function addReceipt()
{
    let index = document.querySelectorAll(".Receipt-contain").length;
    const tempReceipt = document.getElementById("tempReceipt");
    const tempReceiptClone = tempReceipt.content.cloneNode(true);

    tempReceiptClone.querySelector(".ReceiptId").innerText = index + 1;
    tempReceiptClone.querySelector(".Receipt-contain").dataset.index = index;

    const deleteElement = tempReceiptClone.querySelector(".deleteReceipt");
    deleteElement.addEventListener("click", deleteRow);

    const unitElement = tempReceiptClone.querySelector(".unit");
    unitElement.addEventListener("change", unitChange);

    const selectElement = tempReceiptClone.querySelector(".decisionSelect");
    selectElement.addEventListener("change", handleDecisionChange);

    const quantityElement = tempReceiptClone.querySelector(".quantity");
    quantityElement.addEventListener("change", changeTotalValue);

    const priceElement = tempReceiptClone.querySelector(".price");
    priceElement.addEventListener("change", changeTotalValue);

    document.getElementById("TableBody").appendChild(tempReceiptClone);

    result.receiptDetail.push({
        instanceId: null,
        newInstance: null,
        quantity: 0,
        unit: "string",
        price: 0,
        totalValue: 0
    })
}

function deleteRow()
{
    const index = this.parentNode.parentNode.dataset.index;
    const trElement = document.querySelectorAll(".Receipt-contain");
    for (let i = index; i < trElement.length; i++)
    {
        trElement[i].dataset.index = i - 1;
        trElement[i].querySelector(".ReceiptId").innerText = i;
    }

    this.parentNode.parentNode.remove();
    GetTotalValue();
    GetDiscount();
}

function unitChange()
{
    const trElement = this.parentNode.parentNode;
    const index = trElement.dataset.index;
    const unitText = trElement.querySelector(".unit").value

    result.receiptDetail[index].unit = unitText;
}

function changeTotalValue()
{
    const trElement = this.parentNode.parentNode;
    const index = trElement.dataset.index;
    const quantityCount = trElement.querySelector(".quantity").value;
    const priceCount = trElement.querySelector(".price").value;
    const totalValue = trElement.querySelector(".totalValue");
    const totalValueCount = quantityCount * priceCount;
    totalValue.innerText = totalValueCount;

    result.receiptDetail[index].quantity = parseInt(quantityCount);
    result.receiptDetail[index].price = parseInt(priceCount);
    result.receiptDetail[index].totalValue = totalValueCount;

    GetTotalValue();
    GetDiscount();
}

function GetTotalValue()
{
    let sum = 0;
    const totalValue = document.querySelectorAll(".totalValue");
    totalValue.forEach(x =>
    {
        const value = parseFloat(x.innerText)
        sum += value;
    });
    document.querySelector(".total").value = sum;
}

function GetDiscount()
{
    const totalAmount = document.querySelector(".total").value;
    const discountValue = document.querySelector(".discount").value;
    let grandTotal = totalAmount - discountValue
    document.querySelector(".grandTotal").value = grandTotal;
    result.totalAmount = parseInt(totalAmount);
    result.discount = parseInt(discountValue);
    result.totalValue = grandTotal;
}

function handleDecisionChange()
{
    const trElement = this.parentNode.parentNode;
    const selectElement = trElement.querySelector(".decisionSelect");
    const selectedValue = selectElement.options[selectElement.selectedIndex].value;
    const instanceTd = trElement.querySelector(".instance");
    const index = trElement.dataset.index;

    instanceTd.innerHTML = "";

    if (selectedValue === "1")
    {
        const input = document.createElement("input");
        input.type = "text";
        input.className = "inputNewInstance"
        instanceTd.appendChild(input);
        input.addEventListener("change", function ()
        {
            result.receiptDetail[index].instanceId = null
            result.receiptDetail[index].newInstance = trElement.querySelector(".inputNewInstance").value
            trElement.querySelector(".deleteReceipt").disabled = false;

            const rows = document.querySelectorAll(".Receipt-contain");
            const lastRowIndex = rows.length - 1;
            const lastRow = rows[lastRowIndex].querySelector(".instance");
            if (lastRow.hasChildNodes())
            {
                addReceipt()
            } else
                return
        });
    } else if (selectedValue === "2")
    {
        const button = document.createElement("button");
        button.innerText = "เลือกสินค้า";
        button.className = "openModalBtn";

        instanceTd.appendChild(button);

        button.addEventListener("click", function ()
        {
            listIndex = trElement.dataset.index;
            modalOverlay.dataset.index = listIndex;
            modalOverlay.style.display = "flex";
            fetchCategories();
        });
    }
}