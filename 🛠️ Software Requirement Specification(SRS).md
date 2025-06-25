# 📘 Software Requirements Specification (SRS)

## 1. Project Overview

| Element         | Description |
|-----------------|-------------|
| **Project Name** | FloorPlanViz |
| **Purpose**      | Visualize, validate, and export electronic floorplans with connectivity annotations. |
| **Stakeholders** | Architects, RTL designers, Layout Engineers, QA, Product Owner |
| **Scope**        | The tool reads DSL/XML floorplan descriptions, builds a hierarchical model, enables visualization, and validates design rules. |
| **Assumptions**  | Users work on Linux workstations with Python and Qt installed. |
| **Constraints**  | Performance must support 10k+ nodes per view; extensibility required for future integration with other EDA tools. |
---

## 2. Product Requirements

### PRQ_Number

#### Functional Requirements
| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| FR-1 | The system shall parse and load a floorplan described in the DSL format. | High | Input validation included |
| FR-2 | The system shall display the floorplan with interactive zoom and pan features. | High | GUI toolkit: Qt |
| FR-3 | The system shall allow users to search for components by name or type. | Medium | Requires building an index |
| FR-4 | The system shall support exporting the layout as SVG or JSON. | Low | For downstream tools |

---

#### Non-Functional Requirements

| ID | Requirement | Priority |
|----|-------------|----------|
| NFR-1 | The system shall render layouts with <500ms latency for up to 10,000 objects. | High |
| NFR-2 | The tool shall support plugin-based extensions for analysis routines. | High |
| NFR-3 | The system shall allow keyboard navigation and accessible color schemes. | Medium |

---

## 3. Data Requirements

- **Max floorplan nodes**: 10,000+
- **Input file size**: Up to 2MB
- **Export formats**: SVG (static) and JSON (machine-readable)

---


## 4. Appendix
- DSL Grammar specification
- Glossary of floorplan terms



