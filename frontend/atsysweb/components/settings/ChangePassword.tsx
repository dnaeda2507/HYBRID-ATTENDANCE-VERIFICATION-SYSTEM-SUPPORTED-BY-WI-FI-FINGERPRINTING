import {
  ChangePasswordRequest,
  useGetApiVbyVersionUserGetCurrentUserQuery,
  usePostApiAccountChangePasswordMutation,
} from "@/redux/generatedTypes";
import { ChangeEvent, FormEvent, useState } from "react";
import { toast, Toaster } from "react-hot-toast";

const ChangePassword = () => {
  const [form, setForm] = useState({
    oldPassword: "",
    newPassword: "",
    confirmPassword: "",
  });

  const [changePassword, { isLoading }] =
    usePostApiAccountChangePasswordMutation();

  const { data } = useGetApiVbyVersionUserGetCurrentUserQuery({
    version: process.env.NEXT_PUBLIC_API_VERSION as string,
  });

  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();

    if (!form.oldPassword || !form.newPassword) {
      toast.error("Please fill in all fields.");
      return;
    }
    if (form.newPassword !== form.confirmPassword) {
      toast.error("New password and confirm password do not match.");
      return;
    }

    const cmd: ChangePasswordRequest = {
      userId: data?.data?.id,
      oldPassword: form.oldPassword,
      newPassword: form.newPassword,
      confirmPassword: form.confirmPassword,
    };

    const promise = changePassword({
      changePasswordRequest: cmd,
    }).unwrap();
    toast.promise(
      async () => {
        const res = await promise;
        return res;
      },
      {
        loading: "Changing password...",
        success: (res) => {
          if (res.success) {
            setForm({ oldPassword: "", newPassword: "", confirmPassword: "" });
            return res.message ?? "Password changed successfully";
          } else {
            throw new Error(
              `${res?.message ?? ""} \n ${res.errors?.join(", ")}`
            );
          }
        },
        error: (error) => {
          return `Failed to change password: ${error}`;
        },
      }
    );
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6 max-w-md">
      <Toaster
        position="top-right"
        toastOptions={{
          className: "toast",
          duration: 3000,
        }}
      />
      <div>
        <label className="block mb-1">Current password</label>
        <input
          type="password"
          name="oldPassword"
          value={form.oldPassword}
          onChange={handleChange}
          placeholder="••••••••"
          className="w-full px-4 py-2 border-b bg-gray-50"
          required
        />
      </div>

      <div>
        <label className="block mb-1">New password</label>
        <input
          type="password"
          name="newPassword"
          value={form.newPassword}
          onChange={handleChange}
          placeholder="••••••••"
          className="w-full px-4 py-2 border-b bg-gray-50"
          required
        />
      </div>

      <div>
        <label className="block mb-1">Confirm new password</label>
        <input
          type="password"
          name="confirmPassword"
          value={form.confirmPassword}
          onChange={handleChange}
          placeholder="••••••••"
          className="w-full px-4 py-2 border-b bg-gray-50"
          required
        />
      </div>

      <button
        type="submit"
        disabled={isLoading}
        className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 disabled:opacity-50"
      >
        {isLoading ? "Saving…" : "Save"}
      </button>
    </form>
  );
};

export default ChangePassword;
