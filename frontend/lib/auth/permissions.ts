import type { User } from "@/lib/api/types";
export const can = (user: User | null, permission: "manageCatalog" | "manageOperations") => permission === "manageCatalog" ? user?.role === "ClinicAdmin" : ["ClinicAdmin", "Receptionist"].includes(user?.role ?? "");
