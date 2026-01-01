"use client";

import {
  useDeleteApiVbyVersionUserDeleteAndIdMutation,
  useGetApiVbyVersionUserGetAllQuery,
} from "@/redux/generatedTypes";
import React, { useState, useEffect } from "react";
import { Menu, MenuButton, MenuItem, MenuItems } from "@headlessui/react";
import { useRouter } from "next/navigation";
import { toast, Toaster } from "react-hot-toast";

const UserList = () => {
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const router = useRouter();

  useEffect(() => {
    setPageNumber(1);
  }, [pageSize]);

  const { data, isFetching, refetch } = useGetApiVbyVersionUserGetAllQuery({
    pageNumber,
    pageSize,
    version: process.env.NEXT_PUBLIC_API_VERSION as string,
  });

  const [deleteUserAsync] = useDeleteApiVbyVersionUserDeleteAndIdMutation();

  const handleEdit = (id: string) => {
    router.push(`/user/${id}`);
  };

  const handleDelete = (id: string) => {
    if (confirm("Are you sure you want to delete this user?")) {
      const promise = deleteUserAsync({
        id: id,
        version: process.env.NEXT_PUBLIC_API_VERSION as string,
      }).unwrap();

      toast.promise(
        async () => {
          const res = await promise;
          return res;
        },
        {
          loading: "Deleting user...",
          success: (res) => {
            if (res.success) {
              refetch();
              return res.message ?? "User deleted successfully";
            } else {
              throw new Error(
                `${res?.message ?? ""} \n ${res.errors?.join(", ")}`
              );
            }
          },
          error: (error) => {
            return `Failed to delete user: ${error.message}`;
          },
        }
      );
    }
  };

  return (
    <div className="p-6 bg-white rounded-lg shadow h-full">
      <Toaster
        position="top-right"
        toastOptions={{
          className: "toast",
          duration: 3000,
        }}
      />
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-xl font-semibold text-gray-800">Listing</h2>
        <div className="flex items-center space-x-2">
          <div>
            <button
              onClick={() => {
                router.push(`/user/`);
              }}
              className="flex items-center gap-1 px-4 py-2 text-sm font-semibold text-white bg-blue-600 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              Add User
              <svg
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
                strokeWidth={1.5}
                stroke="currentColor"
                className="size-4"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M12 4.5v15m7.5-7.5h-15"
                />
              </svg>
            </button>
          </div>
          <div>
            <label htmlFor="pageSize" className="text-sm text-gray-600">
              Page size:
            </label>
            <select
              id="pageSize"
              value={pageSize}
              onChange={(e) => setPageSize(Number(e.target.value))}
              className="px-2 py-1 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              {[10, 25, 50, 100].map((size) => (
                <option key={size} value={size}>
                  {size}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* loading state */}
      {isFetching ? (
        <p className="text-center text-gray-500">Loading…</p>
      ) : (
        <div className="">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Fullname
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Email
                </th>
                <th className="px-6 py-3 text-end text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {data?.data?.data?.map((user) => (
                <tr key={user.id}>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {user.fullName}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {user.email}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <Menu as="div" className="relative inline-block text-left">
                      <div>
                        <MenuButton className="inline-flex w-full justify-center gap-x-1.5 bg-white px-3 py-2 text-sm font-semibold text-gray-900 hover:bg-gray-50">
                          <svg
                            className="w-5 h-5"
                            aria-hidden="true"
                            xmlns="http://www.w3.org/2000/svg"
                            fill="currentColor"
                            viewBox="0 0 16 3"
                          >
                            <path d="M2 0a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3Zm6.041 0a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3ZM14 0a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3Z" />
                          </svg>
                        </MenuButton>
                      </div>

                      <MenuItems
                        transition
                        className="absolute right-0 z-10 mt-2 w-56 origin-top-right rounded-md bg-white shadow-lg ring-1 ring-black/5 transition focus:outline-hidden data-closed:scale-95 data-closed:transform data-closed:opacity-0 data-enter:duration-100 data-enter:ease-out data-leave:duration-75 data-leave:ease-in"
                      >
                        <div className="py-1">
                          <MenuItem>
                            <button
                              onClick={() => handleEdit(user.id as string)}
                              className="block w-full px-4 py-2 text-left text-sm text-gray-700 hover:bg-gray-100"
                            >
                              Edit
                            </button>
                          </MenuItem>
                          <MenuItem>
                            <button
                              onClick={() => handleDelete(user.id as string)}
                              className="block w-full px-4 py-2 text-left text-sm text-red-600 hover:bg-gray-100"
                            >
                              Delete
                            </button>
                          </MenuItem>
                        </div>
                      </MenuItems>
                    </Menu>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* TODO: Pagination */}
    </div>
  );
};

export default UserList;
