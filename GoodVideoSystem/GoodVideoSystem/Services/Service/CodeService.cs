﻿using GoodVideoSystem.Repositories.IRepository;
using GoodVideoSystem.Services.IService;
using RefactorVideoSystem.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoodVideoSystem.Services.Service
{
    public class CodeService : BaseService, ICodeService
    {
        public readonly string INVALID = "INVALID";         //邀请码无效
        public readonly string UNACTIVE = "UNACTIVE";       //邀请码未激活
        public readonly string AVAILABLE = "AVAILABLE";     //邀请码激活可用
        public readonly string OUTOFTIMES = "OUTOFTIMES";   //邀请码关联设备超过限定值
        public readonly int MAX_DEVICE_COUNT = 5; 
        public readonly int UNACTIVE_ = 0;
        public readonly int ACTIVE_ = 1;
        public readonly int USED_ = 2;


        private ICodeRepository codeRepository { get; set; }
        private IVideoService videoService { get; set; }

        public CodeService(ICodeRepository codeRepository, IVideoService videoService)
        {
            this.codeRepository = codeRepository;
            this.videoService = videoService;
            this.AddDisposableObject(codeRepository);
            this.AddDisposableObject(videoService);
        }

        public IEnumerable<Code> getInviteCodes(string deviceUniqueCode)
        {
            return codeRepository.getInviteCodes(deviceUniqueCode);
        }

        public void addInviteCode(Code code)
        {
            codeRepository.addInviteCode(code);
        }

        public string checkInviteCode(string inviteCode, out Code code)
        {
            code = codeRepository.getInviteCode(inviteCode);
            if (code == null)  //邀请码不存在
                return INVALID;
            if (code.CodeStatus == UNACTIVE_) //邀请码未激活
                return UNACTIVE;
            if (code.BindedDeviceCount >= MAX_DEVICE_COUNT) //邀请码登录设备超过5次
                return OUTOFTIMES;
            return AVAILABLE;
        }

        public void updateInviteCodeInfo(Code inviteCode, string deviceUniqueCode)
        {
            deviceUniqueCode = deviceUniqueCode.Trim();

            if (string.IsNullOrEmpty(deviceUniqueCode))
                return;

            //但凡请求新的视频，需要绑定浏览器指纹到邀请码
            if (inviteCode.BindedDeviceCount < MAX_DEVICE_COUNT)
            {
                inviteCode.CodeStatus = USED_;

                if (inviteCode.DeviceUniqueCode == null)
                    inviteCode.DeviceUniqueCode += ("," + deviceUniqueCode);
                else if(!inviteCode.DeviceUniqueCode.Contains(deviceUniqueCode))
                    inviteCode.DeviceUniqueCode += ("," + deviceUniqueCode);
                inviteCode.BindedDeviceCount = inviteCode.DeviceUniqueCode.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Count();
                codeRepository.updateInviteCode(inviteCode);
            }
        }

        public void updateInviteCodeInfoFromRetrive(Code inviteCode, string deviceUniqueCode)
        {
            deviceUniqueCode = deviceUniqueCode.Trim();

            if (string.IsNullOrEmpty(deviceUniqueCode))
                return;

            //如果指纹数量少于MAX_DEVICE_COUNT，则直接追加上去
            if (inviteCode.BindedDeviceCount < MAX_DEVICE_COUNT)
            {
                inviteCode.CodeStatus = USED_;

                if (inviteCode.DeviceUniqueCode == null)
                    inviteCode.DeviceUniqueCode += ("," + deviceUniqueCode);
                else if (!inviteCode.DeviceUniqueCode.Contains(deviceUniqueCode))
                    inviteCode.DeviceUniqueCode += ("," + deviceUniqueCode);
                inviteCode.BindedDeviceCount = inviteCode.DeviceUniqueCode.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Count();
                codeRepository.updateInviteCode(inviteCode);
            }

            //如果指纹数量对于MAX_DEVICE_COUNT，则清空，再添加
            else
            {
                inviteCode.DeviceUniqueCode = ("," + deviceUniqueCode);
                inviteCode.BindedDeviceCount = 1;
                codeRepository.updateInviteCode(inviteCode);
            }
        }

        public IEnumerable<Code> getInviteCodesContainsCode(string inviteCode, int vid, int pageIndex, int pageSize)
        {
            return codeRepository.getInviteCodes(inviteCode, vid, pageIndex, pageSize, false);
        }

        public IEnumerable<Code> getInviteCodesContainsCode(string inviteCode, int vid)
        {
            return codeRepository.getInviteCodes(inviteCode, vid, false);
        }

        public IEnumerable<Code> getInviteCodesByStatus(int status, int vid, int pageIndex, int pageSize)
        {
            return codeRepository.getInviteCodes(status, vid, pageIndex, pageSize, true);
        }

        public IEnumerable<Code> getInviteCodesByStatus(int status, int vid)
        {
            return codeRepository.getInviteCodes(status, vid, true);
        }

        public IEnumerable<Code> getInviteCodesByUserId(int userid)
        {
            return codeRepository.getInviteCodeByUserId(userid);
        }

        public Code getInviteCodesByCodeValue(string codeValue)
        {
            return codeRepository.getInviteCode(codeValue);
        }

        public void getCounts(int vid, out int codeCount, out int codeCountNotExport, out int codeCountNotUsed, out int codeCountUsed)
        {
            codeRepository.getCounts(vid, out codeCount, out codeCountNotExport, out codeCountNotUsed, out codeCountUsed);
        }

        public Code getInviteCodeById(int id)
        {
            return codeRepository.getInviteCodeById(id);
        }

        public void updateInviteCode(Code code)
        {
            codeRepository.updateInviteCode(code);
        }

        public void deleteInviteCode(Code code)
        {
            if (code == null)
                return;

            //修改视频邀请码数量
            Video video = videoService.getVideo(code.vid);
            if (video == null)
                return;

            int codeCounts, codesNotUsed, codesNotExport, codesUsed;
            getCounts(video.vid, out codeCounts, out codesNotExport, out codesNotUsed, out codesUsed);
            video.CodeNotUsed = codesNotUsed;
            video.CodeUsed = codesUsed;
            video.CodeCounts = codeCounts - 1;

            videoService.updateVideo(video);
            codeRepository.deleteInviteCode(code);
        }

        public void deleteInviteCode(string codeStr)
        {
            Code code = codeRepository.getInviteCode(codeStr.Trim());
            if(code != null)
                codeRepository.deleteInviteCode(code);
        }

        public IEnumerable<Code> getAllInviteCodes()
        {
            return codeRepository.getAllInviteCodes();
        }
    }
}