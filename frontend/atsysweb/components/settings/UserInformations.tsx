"use client";
import {
  UpdateProfileCommand,
  useGetApiVbyVersionUserGetCurrentUserQuery,
  usePutApiVbyVersionUserUpdateProfileMutation,
  UserDetailDto,
} from "@/redux/generatedTypes";
import React, { useState, ChangeEvent, FormEvent, useEffect } from "react";
import { toast, Toaster } from "react-hot-toast";

const UserInformations = () => {
  const [form, setForm] = useState<UserDetailDto>();
  const { data } = useGetApiVbyVersionUserGetCurrentUserQuery({
    version: process.env.NEXT_PUBLIC_API_VERSION as string,
  });

  const [updateProfile] = usePutApiVbyVersionUserUpdateProfileMutation();

  useEffect(() => {
    if (data) {
      setForm({
        ...data?.data,
      });
    }
  }, [data]);

  const handleChange = (
    e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = e.target;
    setForm((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const cmd: UpdateProfileCommand = {
      firstName: form?.firstName,
      lastName: form?.lastName,
      email: form?.email,
      phoneNumber: form?.phoneNumber,
      informationMail: form?.informationMail,
    };
    const promise = updateProfile({
      updateProfileCommand: cmd,
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
            setForm({
              ...form,
              firstName: form?.firstName,
              lastName: form?.lastName,
              email: form?.email,
              phoneNumber: form?.phoneNumber,
              informationMail: form?.informationMail,
            });
            return res.message ?? "User updated successfully";
          } else {
            throw new Error(
              `${res?.message ?? ""} \n ${res.errors?.join(", ")}`
            );
          }
        },
        error: (err) => {
          return `Failed to update user: ${err.message}`;
        },
      }
    );
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <Toaster
        position="top-right"
        toastOptions={{
          className: "toast",
          duration: 3000,
        }}
      />
      {/* Name fields */}
      <div className="flex gap-2">
        <div className="flex-1">
          <label className="block mb-1">First Name</label>
          <input
            value={form?.firstName || ""}
            name="firstName"
            onChange={handleChange}
            placeholder="First Name"
            className="w-full px-4 py-2 border-b bg-gray-50"
            required
          />
        </div>
        <div className="flex-1">
          <label className="block mb-1">Last Name</label>
          <input
            value={form?.lastName || ""}
            name="lastName"
            onChange={handleChange}
            placeholder="Last Name"
            className="w-full px-4 py-2 border-b bg-gray-50"
            required
          />
        </div>
      </div>

      {/* Email */}
      <div>
        <label className="block mb-1">Email</label>
        <input
          type="email"
          value={form?.email || ""}
          name="email"
          onChange={handleChange}
          placeholder="Email"
          className="w-full px-4 py-2 border-b bg-gray-50"
          disabled={true}
          required
        />
      </div>

      {/* Phone Number */}
      <div>
        <label className="block mb-1">Phone Number</label>
        <input
          type="tel"
          value={form?.phoneNumber || ""}
          name="phoneNumber"
          onChange={handleChange}
          placeholder="Phone Number"
          className="w-full px-4 py-2 border-b bg-gray-50"
        />
      </div>

      {/* Information Mail */}
      <div>
        <label className="block mb-1">Information Mail</label>
        <input
          type="email"
          value={form?.informationMail || ""}
          name="informationMail"
          onChange={handleChange}
          placeholder="Information Mail"
          className="w-full px-4 py-2 border-b bg-gray-50"
        />
      </div>

      <button
        type="submit"
        className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700"
      >
        Save
      </button>
    </form>
  );
};

export default UserInformations;
