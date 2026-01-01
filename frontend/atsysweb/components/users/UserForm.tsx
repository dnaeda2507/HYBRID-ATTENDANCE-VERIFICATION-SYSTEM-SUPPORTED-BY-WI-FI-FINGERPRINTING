"use client";

import React, { useEffect, useState, Fragment } from "react";
import { useRouter } from "next/navigation";
import {
  atsysApi,
  CreateUserCommand,
  Roles,
  UpdateUserCommand,
  usePostApiVbyVersionUserCreateMutation,
  usePutApiVbyVersionUserUpdateMutation,
  UserDetailDto,
} from "@/redux/generatedTypes";
import {
  Combobox,
  ComboboxButton,
  ComboboxInput,
  ComboboxOption,
  ComboboxOptions,
  Transition,
} from "@headlessui/react";
import { CheckIcon, ChevronUpDownIcon } from "@heroicons/react/16/solid";
import { toast, Toaster } from "react-hot-toast";

type RoleOption = { id: Roles; name: string };

export function UserForm({ userId }: { userId?: string }) {
  const router = useRouter();
  const [user, setUser] = useState<UserDetailDto>({
    id: "",
    firstName: "",
    lastName: "",
    email: "",
    roles: [],
  });
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const rolesEnum = [
    "It Staff",
    "Teacher",
    "Academic Staff",
    "Student",
  ] as const;
  const roleOptions: RoleOption[] = rolesEnum.map((name, i) => ({
    id: i as Roles,
    name,
  }));

  const [query, setQuery] = useState("");
  const filteredOptions = query
    ? roleOptions.filter((o) =>
        o.name.toLowerCase().includes(query.toLowerCase())
      )
    : roleOptions;

  const [selectedRoles, setSelectedRoles] = useState<RoleOption[]>([]);

  const [fetchById, { isLoading: loadingUser }] =
    atsysApi.endpoints.getApiVbyVersionUserGetByIdAndId.useLazyQuery();
  const [refetch] =
    atsysApi.endpoints.getApiVbyVersionUserGetAll.useLazyQuerySubscription();
  const [createUser] = usePostApiVbyVersionUserCreateMutation();
  const [updateUser] = usePutApiVbyVersionUserUpdateMutation();

  useEffect(() => {
    if (!userId) return;
    const promise = fetchById({
      id: userId,
      version: process.env.NEXT_PUBLIC_API_VERSION!,
    }).unwrap();

    toast.promise(
      async () => {
        const res = await promise;
        return res;
      },
      {
        loading: "Loading user...",
        success: (res) => {
          if (res.success) {
            setUser(res?.data || {});
            setSelectedRoles(
              roleOptions.filter((opt) => res?.data?.roles?.includes(opt.id))
            );
            return res.message ?? "User loaded successfully";
          } else {
            throw new Error(
              `${res?.message ?? ""} \n ${res.errors?.join(", ")}`
            );
          }
        },
        error: (error) => {
          return `Failed to load user: ${error.message}`;
        },
      }
    );
  }, [userId, fetchById]);

  useEffect(() => {
    setUser((u) => ({ ...u, roles: selectedRoles.map((r) => r.id) }));
  }, [selectedRoles]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const payloadRoles = selectedRoles.map((r) => r.id);

    if (userId) {
      const cmd: UpdateUserCommand = {
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email,
        roles: payloadRoles,
      };
      const promise = updateUser({
        updateUserCommand: cmd,
        version: process.env.NEXT_PUBLIC_API_VERSION!,
      }).unwrap();

      toast.promise(
        async () => {
          const res = await promise;
          return res;
        },
        {
          loading: "Updating user...",
          success: (res) => {
            if (res.success) {
              refetch({
                pageNumber: 1,
                pageSize: 10,
                version: process.env.NEXT_PUBLIC_API_VERSION!,
              });
              router.push("/users");
              return res.message ?? "User updated successfully";
            } else {
              throw new Error(
                `${res?.message ?? ""} \n ${res.errors?.join(", ")}`
              );
            }
          },
          error: (error) => {
            return `Failed to update user: ${error.message}`;
          },
        }
      );
    } else {
      if (password !== confirmPassword) {
        return toast.error("Passwords do not match");
      }
      const cmd: CreateUserCommand = {
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email,
        roles: payloadRoles,
        password,
      };
      const promise = createUser({
        createUserCommand: cmd,
        version: process.env.NEXT_PUBLIC_API_VERSION!,
      }).unwrap();

      toast.promise(
        async () => {
          const res = await promise;
          return res;
        },
        {
          loading: "Creating user...",
          success: (res) => {
            if (res.success) {
              refetch({
                pageNumber: 1,
                pageSize: 10,
                version: process.env.NEXT_PUBLIC_API_VERSION!,
              });
              router.push("/users");
              return res.message ?? "User created successfully";
            } else {
              throw new Error(
                `${res?.message ?? ""} \n ${res.errors?.join(", ")}`
              );
            }
          },
          error: (error) => {
            return `Failed to create user: ${error.message}`;
          },
        }
      );
    }
  };

  if (loadingUser) {
    return (
      <div className="flex items-center justify-center h-screen">Loading…</div>
    );
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-4 w-1/2 p-6 bg-white rounded shadow"
    >
      <Toaster
        position="top-right"
        toastOptions={{
          className: "toast",
          duration: 3000,
        }}
      />
      <h1 className="text-xl mb-4">{userId ? "Edit User" : "Create User"}</h1>

      {/* Name fields */}
      <div className="flex gap-2">
        <div className="flex-1">
          <label>First Name</label>
          <input
            value={user.firstName || ""}
            onChange={(e) =>
              setUser((u) => ({ ...u, firstName: e.target.value }))
            }
            placeholder="First Name"
            className="w-full px-4 py-2 border-b bg-gray-50"
            required
          />
        </div>
        <div className="flex-1">
          <label>Last Name</label>
          <input
            value={user.lastName || ""}
            onChange={(e) =>
              setUser((u) => ({ ...u, lastName: e.target.value }))
            }
            placeholder="Last Name"
            className="w-full px-4 py-2 border-b bg-gray-50"
            required
          />
        </div>
      </div>

      {/* Email */}
      <div>
        <label>Email</label>
        <input
          type="email"
          value={user.email || ""}
          onChange={(e) => setUser((u) => ({ ...u, email: e.target.value }))}
          placeholder="Email"
          className="w-full px-4 py-2 border-b bg-gray-50"
          required
        />
      </div>

      {/* Roles Combobox */}
      <div>
        <label>Select Role(s)</label>
        <Combobox
          value={selectedRoles}
          onChange={(opts: RoleOption[]) => setSelectedRoles(opts)}
          multiple
        >
          <div className="relative">
            <ComboboxInput
              className="w-full px-3 py-2 border-b bg-gray-50"
              displayValue={(opts: RoleOption[]) =>
                opts.map((o) => o.name).join(", ")
              }
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Pick one or more roles…"
            />
            <ComboboxButton className="absolute right-0 top-0 px-2 py-2">
              <ChevronUpDownIcon className="h-5 w-5" />
            </ComboboxButton>
            <Transition
              as={Fragment}
              leave="ease-in duration-100"
              leaveFrom="opacity-100"
              leaveTo="opacity-0"
            >
              <ComboboxOptions className="absolute mt-1 max-h-60 w-full overflow-auto bg-white shadow-lg">
                {filteredOptions.length === 0 ? (
                  <div className="p-2 text-gray-700">Nothing found.</div>
                ) : (
                  filteredOptions.map((opt) => (
                    <ComboboxOption key={opt.id} value={opt}>
                      {({ selected, active }) => (
                        <div
                          className={`flex items-center p-2 ${active ? "bg-blue-600 text-white" : ""}`}
                        >
                          <span className={selected ? "font-semibold" : ""}>
                            {opt.name}
                          </span>
                          {selected && (
                            <CheckIcon className="ml-auto h-5 w-5" />
                          )}
                        </div>
                      )}
                    </ComboboxOption>
                  ))
                )}
              </ComboboxOptions>
            </Transition>
          </div>
        </Combobox>
      </div>

      {/* Passwords for create */}
      {!userId && (
        <div className="flex gap-2">
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Password"
            className="flex-1 px-4 py-2 border-b bg-gray-50"
            required
          />
          <input
            type="password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            placeholder="Confirm Password"
            className="flex-1 px-4 py-2 border-b bg-gray-50"
            required
          />
        </div>
      )}

      <div className="text-right">
        <button
          type="submit"
          className="px-4 py-2 bg-blue-600 text-white rounded"
        >
          {userId ? "Update User" : "Create User"}
        </button>
      </div>
    </form>
  );
}
