const urlParams = new URLSearchParams(window.location.search);
const employeeId = urlParams.get("id");
const apiUrl = `http://localhost:5143/Employee/GetById?id=${employeeId}`;

async function fetchEmployeeData() {
  try {
    const response = await fetch(apiUrl);
    const result = await response.json();

    if (response.ok) {
      populateEmployeeInfo(result);
    } else {
      alert(result.Message || "Failed to fetch employee data.");
    }
  } catch (error) {
    console.error("Error fetching employee data:", error);
  }
}
async function fetchFreeItemData() {
  try {
    const response = await fetch(`http://localhost:5143/Item/GetFreeItems`);
    const result = await response.json();

    if (response.ok) {
      FreeItemInfo(result);
    } else {
      alert(result.Message || "Failed to fetch free items.");
    }
  } catch (error) {
    console.error("Error fetching free items:", error);
  }
}

function FreeItemInfo(data) {
  const freeItemSelect = document.getElementById("freeItem");
  freeItemSelect.innerHTML = "";
  data.forEach((item) => {
    const option = document.createElement("option");
    option.value = item.itemInstanceId;
    option.textContent = `(${item.classificationName}) ${item.assetId}`;
    freeItemSelect.appendChild(option);
  });
}

function populateEmployeeInfo(data) {
  document.getElementById(
    "employeeName"
  ).textContent = `Name: ${data.fullName}`;

  const tableBody = document.getElementById("employeeTableBody");
  tableBody.innerHTML = "";
  data.requisitionedItems.forEach((item) => {
    const row = document.createElement("tr");
    row.innerHTML = `
                    <td>${item.itemCategoryName}</td>
                    <td>${item.itemClassificationName}</td>
                    <td>${item.assetId}</td>
                    <td>${item.requisitonDate}</td>
                    <td>
                        <button onclick="returnItem(${item.requisitionId})">Return</button>
                    </td>
                `;
    tableBody.appendChild(row);
  });
}

document
  .getElementById("requisitionItem")
  .addEventListener("click", async () => {
    const selectedItemId = document.getElementById("freeItem").value;

    if (!selectedItemId) {
      alert("Please select an item to requisition.");
      return;
    }

    try {
      const response = await fetch(
        "http://localhost:5143/Requisitoned/Borrow",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            EmployeeId: employeeId,
            ItemInstanceId: selectedItemId,
          }),
        }
      );

      const result = await response.json();

      if (response.ok) {
        fetchEmployeeData();
        fetchFreeItemData();
      } else {
        alert(result.Message || "Failed to requisition item.");
      }
    } catch (error) {
      console.error("Error requisitioning item:", error);
    }
  });

async function returnItem(requisitionId) {
  try {
    const response = await fetch("http://localhost:5143/Requisitoned/Return", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ RequisitionId: requisitionId }),
    });

    const result = await response.json();

    if (response.ok) {
      fetchEmployeeData();
      fetchFreeItemData();
    } else {
      alert(result.Message || "Failed to return item.");
    }
  } catch (error) {
    console.error("Error returning item:", error);
  }
}

document.getElementById("editEmployee").addEventListener("click", () => {
  window.location.href = `editEmployee.html?id=${employeeId}`;
});
document
  .getElementById("deleteEmployee")
  .addEventListener("click", async () => {
    const confirmDelete = confirm(
      "Are you sure you want to delete this employee?"
    );
    if (!confirmDelete) return;

    try {
      const payload = { Id: employeeId };

      const response = await fetch("http://localhost:5143/Employee/Delete", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });

      const result = await response.json();

      if (response.ok && result.statusCode === 200) {
        alert(result.message || "Employee deleted successfully.");
        window.location.href = "employee.html";
      } else {
        alert(result.message || "Failed to delete employee.");
      }
    } catch (error) {
      console.error("Error deleting employee:", error);
      alert("An error occurred while deleting the employee.");
    }
  });

fetchEmployeeData();
fetchFreeItemData();
