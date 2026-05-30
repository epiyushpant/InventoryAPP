# ABC Company: Inventory Management Strategic Plan

## 1. System Usage Workflow
Follow this sequence to maintain perfect data integrity:

1.  **Definitions**: Setup **Locations** and **Categories**.
2.  **Parties**: Add **Suppliers** and **Customers**.
3.  **Catalog**: Add **Products** (link to Category & default Supplier).
4.  **Procurement**: 
    - Create **Purchase Order**.
    - Add **PO Details**.
    - Mark as **"Completed"** (Stock increases automatically).
5.  **Sales**:
    - Create **Sale**.
    - Add **Sale Details**.
    - Mark as **"Completed"** (Stock decreases automatically).

## 2. Architectural Improvements

### Phase 1: Data Integrity (Current Focus)
- [ ] **Stock Validation**: Prevent completing a Sale if inventory is insufficient.
- [ ] **Transactional Safety**: Ensure header and details updates happen together.

### Phase 2: User Experience (Next)
- [ ] **Unified Forms**: Create/Edit POs and Sales with line items on a single screen.
- [ ] **Summary Dashboards**: View low stock and revenue at a glance.

### Phase 3: Advanced Automation
- [ ] **Auto-Reorder**: Generate draft POs when stock hits `ReorderLevel`.
- [ ] **Price History**: Track changing unit costs from suppliers over time.

## 3. Maintenance Tips
- Always use the **"Completed"** status to trigger inventory updates.
- Assign a **Default Supplier** to every product to enable auto-procurement logic.
- Periodically check the **Stock Movements** log to audit any discrepancies.
